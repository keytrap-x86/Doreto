#pragma once
#include <WinSock2.h>

// Definitions of the original functions to detour
int (WINAPI* pSend)(SOCKET s, const char* buf, int len, int flags) = send;
int (WINAPI* pRecv)(SOCKET s, char* buf, int len, int flags) = recv;
int (WINAPI* pConnect)(SOCKET s, const sockaddr* name, int namelen) = connect;

// Global variables
SOCKET g_Socket = 0; // Stores the actual game's socket
char g_ServerIp[INET_ADDRSTRLEN]; // Stores the server IP address
HMODULE g_Module; // Current dll instance
HANDLE g_PipeServer; // Pipe server instance
BOOL g_IsClientConnected; // Is client connected to pipe server
BOOL g_IsPipeServerWaitingForClients; // Is server waiting for pipe client