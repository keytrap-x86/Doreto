#include "pch.h"
#include "handlefinder.h"

// Credits go to: https://blez.wordpress.com/2012/09/17/enumerating-opened-handles-from-a-process/

PVOID GetLibraryProcAddress(PSTR LibraryName, PSTR ProcName)
{
	return GetProcAddress(GetModuleHandleA(LibraryName), ProcName);
}

/// <summary>
///		Enumerates all handles opened by the current process.
///		Finds the handle of socket type (File) and returns the one that is connected to the given IP address.
/// </summary>
/// <param name="ipaddr"></param>
/// <returns></returns>
SOCKET FindSocketByIpAddr(char* ipaddr) {
	_NtQuerySystemInformation NtQuerySystemInformation = (_NtQuerySystemInformation)GetLibraryProcAddress("ntdll.dll", "NtQuerySystemInformation");
	_NtDuplicateObject NtDuplicateObject = (_NtDuplicateObject)GetLibraryProcAddress("ntdll.dll", "NtDuplicateObject");
	_NtQueryObject NtQueryObject = (_NtQueryObject)GetLibraryProcAddress("ntdll.dll", "NtQueryObject");

	NTSTATUS status;
	PSYSTEM_HANDLE_INFORMATION handleInfo;
	ULONG handleInfoSize = 0x10000;
	ULONG pid;
	HANDLE processHandle;
	ULONG i;

	SOCKET sock = INVALID_SOCKET;

	pid = GetCurrentProcessId();

	if (!(processHandle = OpenProcess(PROCESS_DUP_HANDLE, FALSE, pid))) {
		printf("Could not open PID %d! (Don't try to open a system process.)\n", pid);
		return 1;
	}

	handleInfo = (PSYSTEM_HANDLE_INFORMATION)malloc(handleInfoSize);

	// NtQuerySystemInformation won't give us the correct buffer size,
	//  so we guess by doubling the buffer size.
	while ((status = NtQuerySystemInformation(
		SystemHandleInformation,
		handleInfo,
		handleInfoSize,
		NULL
	)) == STATUS_INFO_LENGTH_MISMATCH)
		handleInfo = (PSYSTEM_HANDLE_INFORMATION)realloc(handleInfo, handleInfoSize *= 2);

	// NtQuerySystemInformation stopped giving us STATUS_INFO_LENGTH_MISMATCH.
	if (!NT_SUCCESS(status)) {
		printf("NtQuerySystemInformation failed!\n");
		return 1;
	}

	for (i = 0; i < handleInfo->HandleCount; i++) {
		SYSTEM_HANDLE handle = handleInfo->Handles[i];
		HANDLE dupHandle = NULL;
		POBJECT_TYPE_INFORMATION objectTypeInfo;
		PVOID objectNameInfo;
		UNICODE_STRING objectName;
		ULONG returnLength;

		// Check if this handle belongs to the PID the user specified.
		if (handle.ProcessId != pid)
			continue;

		// Duplicate the handle so we can query it.
		if (!NT_SUCCESS(NtDuplicateObject(
			processHandle,
			(void*)handle.Handle,
			GetCurrentProcess(),
			&dupHandle,
			0,
			0,
			0
		))) {

			continue;
		}

		// Query the object type.
		objectTypeInfo = (POBJECT_TYPE_INFORMATION)malloc(0x1000);
		if (!NT_SUCCESS(NtQueryObject(
			dupHandle,
			ObjectTypeInformation,
			objectTypeInfo,
			0x1000,
			NULL
		))) {

			CloseHandle(dupHandle);
			continue;
		}

		// Query the object name (unless it has an access of
		//   0x0012019f, on which NtQueryObject could hang.
		if (handle.GrantedAccess == 0x0012019f) {
			free(objectTypeInfo);
			CloseHandle(dupHandle);
			continue;
		}

		objectNameInfo = malloc(0x1000);
		if (!NT_SUCCESS(NtQueryObject(
			dupHandle,
			ObjectNameInformation,
			objectNameInfo,
			0x1000,
			&returnLength
		))) {

			// Reallocate the buffer and try again.
			objectNameInfo = realloc(objectNameInfo, returnLength);
			if (!NT_SUCCESS(NtQueryObject(
				dupHandle,
				ObjectNameInformation,
				objectNameInfo,
				returnLength,
				NULL
			))) {

				free(objectTypeInfo);
				free(objectNameInfo);
				CloseHandle(dupHandle);
				continue;
			}
		}

		// Cast our buffer into an UNICODE_STRING.
		objectName = *(PUNICODE_STRING)objectNameInfo;

		// Print the information!
		if (objectName.Length)
		{
			// Check if the object name is \Device\Afd
			if (wcscmp(objectName.Buffer, L"\\Device\\Afd") == 0)
			{
				auto socket = (SOCKET)handle.Handle;

				// Check if socket is valid
				if (socket == INVALID_SOCKET)
				{
					free(objectTypeInfo);
					free(objectNameInfo);
					CloseHandle(dupHandle);
					continue;
				}

				SOCKADDR_IN client_info = { 0 };
				int client_info_size = sizeof(client_info);
				getpeername(socket, (SOCKADDR*)&client_info, &client_info_size);
				char* ip = inet_ntoa(client_info.sin_addr);
				//printf("Socket handle: 0x%08.8X\n", socket);
				//printf("Socket found: %s\n", ip);

				// Check if socket sin_addr is equal to 192.168.1.84
				if (strcmp(ip, ipaddr) == 0) {
					free(objectTypeInfo);
					free(objectNameInfo);
					CloseHandle(dupHandle);
					return socket;
				}
			}
		}

		free(objectTypeInfo);
		free(objectNameInfo);
		CloseHandle(dupHandle);
	}

	free(handleInfo);
	CloseHandle(processHandle);

	return sock;
}