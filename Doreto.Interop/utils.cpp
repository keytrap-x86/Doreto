#include "pch.h"
#include "utils.h"
#include <stdio.h>
#include <io.h>
#include <fcntl.h>
#include <WinSock2.h>
#include <ws2tcpip.h>

/// <summary>
///		Use for debug purpose. It attaches a console after the current DLL has been injected.
///		The console displays all the packets from send/recv hooks.
/// </summary>
void utils::CreateConsole()
{
	if (!AllocConsole())
	{
		char buffer[BUFF_SIZE] = { 0 };
		sprintf_s(buffer, "Failed to AllocConsole( ), GetLastError( ) = %d", GetLastError());
		return;
	}

	auto lStdHandle = GetStdHandle(STD_OUTPUT_HANDLE);
	auto hConHandle = _open_osfhandle(PtrToUlong(lStdHandle), _O_TEXT);
	auto fp = _fdopen(hConHandle, "w");

	freopen_s(&fp, "CONOUT$", "w", stdout);
	freopen_s(&fp, "CONOUT$", "w", stderr);

	*stdout = *fp;
	setvbuf(stdout, NULL, _IONBF, 0);
	setvbuf(stderr, NULL, _IONBF, 0);
}

/// <summary>
///		Use for debug purpose. It detaches the console after the current DLL has been injected.
/// </summary>
/// <returns></returns>
bool utils::ReleaseConsole()
{
	bool result = true;
	FILE* fp;

	// Just to be safe, redirect standard IO to NUL before releasing.

	// Redirect STDIN to NUL
	if (freopen_s(&fp, "NUL:", "r", stdin) != 0)
		result = false;
	else
		setvbuf(stdin, NULL, _IONBF, 0);

	// Redirect STDOUT to NUL
	if (freopen_s(&fp, "NUL:", "w", stdout) != 0)
		result = false;
	else
		setvbuf(stdout, NULL, _IONBF, 0);

	// Redirect STDERR to NUL
	if (freopen_s(&fp, "NUL:", "w", stderr) != 0)
		result = false;
	else
		setvbuf(stderr, NULL, _IONBF, 0);

	// Detach from console
	while (!FreeConsole())
	{
		Sleep(500);
	}

	result = true;

	return result;
}

/// <summary>
///		Creates a two way pipe server 
/// </summary>
/// <param name="pipeName"></param>
/// <returns></returns>
HANDLE utils::CreatePipeServer(LPCSTR pipeName)
{
	return CreateNamedPipe(
		pipeName, // name of the pipe
		PIPE_ACCESS_DUPLEX, // 2-way pipe -- send/receive
		PIPE_TYPE_MESSAGE | PIPE_READMODE_MESSAGE | PIPE_NOWAIT, // send data as a message & don't block
		1, // only allow 1 instance of this pipe
		BUFF_SIZE, // outbound buffer
		BUFF_SIZE, // inbound buffer
		0, // default wait time
		NULL // not overlapped
	);
}

/// <summary>
///		Check if the string contains only printable characters
/// </summary>
/// <param name="filePath"></param>
/// <returns></returns>
bool utils::ContainsOnlyASCII(const std::string& filePath) {
	for (auto c : filePath) {
		if (static_cast<unsigned char>(c) > 127) {
			return false;
		}
	}
	return true;
}

/// <summary>
///		Checks if the char array is null terminated
/// </summary>
/// <param name="str"></param>
/// <returns></returns>
bool utils::IsNullTerminated(const char* str)
{
	int len = strlen(str);
	for (int i = 0; i < len; ++i) {
		if (str[i] == '0')
			return true;
	}

	return false;
}

/// <summary>
///		Appends a null terminator to the string
///		and writes it to the file
/// </summary>
void utils::WriteToPipe(HANDLE pipe, const char* buf)
{
	DWORD bytesWritten = 0;
	
	// Append null terminator to the string
	char* bufWithNull = new char[strlen(buf) + 1];
	strcpy_s(bufWithNull, strlen(buf) + 1, buf);
	bufWithNull[strlen(buf)] = '\0';
		
	WriteFile(pipe, buf, strlen(buf), &bytesWritten, NULL);
}

/// <summary>
///		Gets the ip address from a socket.
/// </summary>
/// <param name="socket"></param>
/// <returns></returns>
string utils::GetIpFromSocket(SOCKET socket)
{
	struct sockaddr_in   address = { 0 };
	socklen_t            addressLength = sizeof(address);
	std::string               ip;

	int result = getpeername(socket, (struct sockaddr*)&address, &addressLength);
	ip = inet_ntoa(address.sin_addr);

	return ip.c_str();
}

