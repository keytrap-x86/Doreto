#pragma once
#include <winnt.h>
#include <string>
#include <WinSock2.h>

using namespace std;
constexpr auto BUFF_SIZE = 1024;

/// <summary>
///		Contains helper methods
/// </summary>
namespace utils
{
	void CreateConsole();
	bool ReleaseConsole();
	HANDLE CreatePipeServer(LPCSTR pipeName);
	bool ContainsOnlyASCII(const std::string& filePath);
	bool IsNullTerminated(const char* str);
	void WriteToPipe(HANDLE pipe, const char* buf);
	string GetIpFromSocket(SOCKET socket);
}