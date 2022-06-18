// dllmain.cpp : Defines the entry point for the DLL application.
#include "pch.h"
#include <WinSock2.h>
#include "detours.h"
#include <cstdio>
#include <string>
#include <iostream>
#include <stdio.h>
#include <strsafe.h>
#include "utils.h"
#include <ws2tcpip.h>
#include <algorithm>
#include "spdlog/spdlog.h"
#include "spdlog/sinks/stdout_color_sinks.h"
#include "handlefinder.h"
#include "dofus_messages.h"
#include "common.h"

using namespace std;

// Load detour libs
#if _WIN32 || _WIN64
#if _WIN64
#define ENVIRONMENT64
#pragma comment(lib,"detours64.lib")
#else
#define ENVIRONMENT32
#pragma comment(lib,"detours32.lib")
#endif
#endif

// Load winsock2 lib
#pragma comment(lib,"ws2_32.lib")



// Detoured send
int WINAPI MySend(SOCKET s, const char* buf, int len, int flags)
{
	// Update socket and ip
	auto ip = utils::GetIpFromSocket(s);
	strcpy_s(g_ServerIp, ip.c_str());
	g_Socket = s;

	std::string str = buf;

	// Call the original function
	int result = pSend(s, buf, len, flags);

	if (g_IsClientConnected)
	{
		// Check if buffer contains only ASCII characters
		// because otherwise it will send random sh*t
		if (utils::ContainsOnlyASCII(str))
		{
			spdlog::info("[SEND] {}", str);
			utils::WriteToPipe(g_PipeServer, buf);
		}
	}
	return result;
}

// Detoured recv
int WINAPI MyRecv(SOCKET s, char* buf, int len, int flags)
{
	// Update socket and ip
	auto ip = utils::GetIpFromSocket(s);
	strcpy_s(g_ServerIp, ip.c_str());
	g_Socket = s;

	std::string str = buf;

	// Call the original recv
	int result = pRecv(s, buf, len, flags);

	if (g_IsClientConnected)
	{
		// Check if buffer contains only ASCII characters
		// because otherwise it will send random sh*t
		if (utils::ContainsOnlyASCII(str))
		{
			spdlog::info("[RECV] {0} {1:x}", buf, s);
			utils::WriteToPipe(g_PipeServer, buf);
		}
	}
	return result;
}

// Detoured connect
int WINAPI MyConnect(SOCKET s, const sockaddr* name, int namelen)
{

	int result = pConnect(s, name, namelen);
	
	// Update socket and ip
	auto ip = utils::GetIpFromSocket(s);
	strcpy_s(g_ServerIp, ip.c_str());
	g_Socket = s;

	spdlog::info("[CONNECT] Socket: {0:x}", g_Socket);

	return result;
}


// ---------- PIPE SERVER THREAD ----------
DWORD WINAPI RunPipeServer(LPVOID lpModule)
{
	// Create the console and initialize logger
	utils::CreateConsole();
	auto console_sink = std::make_shared<spdlog::sinks::stdout_color_sink_mt>();
	spdlog::set_default_logger(std::make_shared<spdlog::logger>("", spdlog::sinks_init_list({ console_sink })));

	spdlog::info("Pipe server started");

	// Gets the current process id
	DWORD curr_proc_id = GetCurrentProcessId();

	// Declare an empty pipe server
	g_PipeServer = NULL;

	// Concat the current process id to the pipe name
	char pipeName[256] = "\\\\.\\pipe\\doreto_";
	strcat_s(pipeName, 256, std::to_string(curr_proc_id).c_str());

	// Starting pipe server
	while (g_PipeServer == NULL || g_PipeServer == INVALID_HANDLE_VALUE)
	{
		// Create a pipe to send data
		g_PipeServer = utils::CreatePipeServer(pipeName);
	}

	// Infinite loop to wait for client to connect
	while (!g_IsClientConnected)
	{
		// This blocks until a new client connects
		g_IsClientConnected = ConnectNamedPipe(g_PipeServer, NULL) ? TRUE : (GetLastError() == ERROR_PIPE_CONNECTED);

		if (g_IsClientConnected)
		{
			spdlog::info("New client connected");
			g_IsClientConnected = TRUE;
			g_IsPipeServerWaitingForClients = FALSE;
			

			// Prepare an empty buffer to read from the pipe
			char data[BUFF_SIZE]{};

			while (true)
			{
				DWORD read;

				// Read the data from pipe. This blocks until data is available
				bool success = ReadFile(g_PipeServer, data, BUFF_SIZE, &read, nullptr);

				if (success)
				{
					// Check if the data is equal to "!dettach"
					if (strstr(data, "!dettach") != NULL) {
						spdlog::info("Dettaching from client");
						break;
					}
					else if (strstr(data, "!getserver") != NULL) {
						// If g_ServerIp has value, send "!setserver:" + the server ip through the pipe
						if (strlen(g_ServerIp) > 0) {
							char buf[BUFF_SIZE]{};
							strcpy_s(buf, "!setserver:");
							strcat_s(buf, BUFF_SIZE, g_ServerIp);
							utils::WriteToPipe(g_PipeServer, buf);
						}
					}
					// Else check if the data starts with "!findsocketfor:"
					else if (strstr(data, "!findsocketfor:") != NULL) {
						// Get everything after "!findsocketfor:"
						char* socket_str = strstr(data, "!findsocketfor:") + strlen("!findsocketfor:");
						spdlog::info("Finding socket for ip {0}", socket_str);
						auto socket = FindSocketByIpAddr(socket_str);
						spdlog::info("Socket : {0:x}", socket);
						g_Socket = socket;

						// Send BW\n\0 to the socket
						char buf[BUFF_SIZE]{};
						strcpy_s(buf, WHOAMI);
						MySend(socket, buf, strlen(buf) + 1, 0);
					}
					else {
						// Check if the socket is valid and send the data
						if (g_Socket != INVALID_SOCKET)
						{
							int len = MySend(g_Socket, data, read, 0);
							spdlog::debug("Bytes send {}", len);
						}
					}
				}
				else {
					// If client has disconnected, break the loop
					if (GetLastError() == ERROR_BROKEN_PIPE)
					{
						g_IsClientConnected = FALSE;
						break;
					}
				}
			}
		}
		else
		{
			// Here the client has disconnected. Wait for a new client to connect.
			
			Sleep(1000);

			// This is just to avoid printing the message too often
			if (g_IsPipeServerWaitingForClients == FALSE)
			{
				spdlog::info("Waiting for client to connect...");
				g_IsPipeServerWaitingForClients = TRUE;
			}

			// When the client has disconnected, wait for a new client
			g_IsClientConnected = FALSE;

			DWORD err = GetLastError();
			// When the client disconnects, the pipe is broken (but is still waiting for a new client)
			// We ignore the ERROR_PIPE_LISTENING error because it is not really an error
			// It just means that the client has disconnected and the pipe is waiting for a new client
			// If it's not the case, we print the error
			if (err > 0 && err != ERROR_PIPE_LISTENING)
			{
				spdlog::error("{}", err);
				if (err == ERROR_NO_DATA) {
					DisconnectNamedPipe(g_PipeServer);
				}
			}
		}
	}


	// Free pipe
	CloseHandle(g_PipeServer);

	// Free the console
	utils::ReleaseConsole();

	// Exit the thread (dll will be unloaded)
	FreeLibraryAndExitThread((HMODULE)lpModule, 0);
	return 0;
}



// ---------- DLL ENTRY POINT ----------
BOOL APIENTRY DllMain(HMODULE hModule, DWORD  ul_reason_for_call, LPVOID lpReserved)
{

	if (DetourIsHelperProcess()) {
		return TRUE;
	}

	if (ul_reason_for_call == DLL_PROCESS_ATTACH) { // DLL is loaded into the process

		DisableThreadLibraryCalls(hModule);

		// The following functions will will create detours
		// send
		DetourTransactionBegin();
		DetourUpdateThread(GetCurrentThread());
		DetourAttach(&(PVOID&)pRecv, MyRecv); // pRecv is a pointer to the function that will be detoured, MyRecv is the function that will replace it
		DetourTransactionCommit();
		// recv
		DetourTransactionBegin();
		DetourUpdateThread(GetCurrentThread());
		DetourAttach(&(PVOID&)pSend, MySend); // ...
		DetourTransactionCommit();
		// connect
		DetourTransactionBegin();
		DetourUpdateThread(GetCurrentThread());
		DetourAttach(&(PVOID&)pConnect, MyConnect); // ...
		DetourTransactionCommit();

		// Create the thread for the pipe server
		CreateThread(NULL, 0, RunPipeServer, hModule, NULL, NULL);
	}
	else if (ul_reason_for_call == DLL_PROCESS_DETACH) {

		DetourTransactionBegin();
		DetourUpdateThread(GetCurrentThread());
		DetourDetach(&(PVOID&)pSend, MySend);
		DetourTransactionCommit();

		DetourTransactionBegin();
		DetourUpdateThread(GetCurrentThread());
		DetourDetach(&(PVOID&)pRecv, MyRecv);
		DetourTransactionCommit();

		DetourTransactionBegin();
		DetourUpdateThread(GetCurrentThread());
		DetourDetach(&(PVOID&)pConnect, MyConnect);
		DetourTransactionCommit();

	}
	return TRUE;
}

