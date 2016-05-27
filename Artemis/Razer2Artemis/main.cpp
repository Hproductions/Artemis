// Original work by VRocker https://github.com/VRocker/LogiLed2Corsair
// I'm mainly a C# developer, and these modification aren't a piece of art, but it suits our needs.

// The MIT License (MIT)
// 
// Copyright (c) 2015 VRocker
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

#include "main.h"
#include <stdio.h>
#include <tchar.h>
#include <thread>
#define WIN32_LEAN_AND_MEAN 
#include <Windows.h>
#include "Logger.h"

#include <filesystem>


static bool g_hasInitialised = false;
const char* game = "";

void cleanup()
{
	CLogger::EndLogging();
}

BOOL WINAPI DllMain(HINSTANCE hInst, DWORD fdwReason, LPVOID)
{
	switch (fdwReason)
	{
	case DLL_PROCESS_ATTACH:
	{
		atexit(cleanup);

		CLogger::InitLogging("Log.txt");
		CLogger::SetLogLevel(LogLevel::Debug);

		// Get the process that loaded the DLL
		TCHAR overwatchFind[] = _T("Overwatch");
		TCHAR szPath[MAX_PATH];
		GetModuleFileName(NULL, szPath, MAX_PATH);

		if (_tcscmp(szPath, overwatchFind) != 0)
			game = "overwatch";

		CLogger::OutputLog("Attached to process.", LogLevel::Debug);
	}
	break;

	case DLL_PROCESS_DETACH:
	{
		cleanup();

		CLogger::OutputLog_s("Detached from process.", LogLevel::Debug);
	}
	break;
	}

	return true;
}

RZRESULT Init()
{
	CLogger::OutputLog_s("Razer Init called.", LogLevel::Debug);
	g_hasInitialised = true;
	return 0;
}

RZRESULT UnInit()
{
	CLogger::OutputLog_s("Razer UnInit called.", LogLevel::Debug);
	return 0;
}

RZRESULT CreateEffect(RZDEVICEID DeviceId, ChromaSDK::EFFECT_TYPE Effect, PRZPARAM pParam, RZEFFECTID* pEffectId)
{
	CLogger::OutputLog_s("Razer CreateEffect called.", LogLevel::Debug);
	return 0;
}

RZRESULT CreateKeyboardEffect(ChromaSDK::Keyboard::EFFECT_TYPE Effect, PRZPARAM pParam, RZEFFECTID* pEffectId)
{
	CLogger::OutputLog_s("Razer CreateKeyboardEffect called.", LogLevel::Debug);
	return 0;
}

RZRESULT CreateMouseEffect(ChromaSDK::Mouse::EFFECT_TYPE Effect, PRZPARAM pParam, RZEFFECTID* pEffectId)
{
	CLogger::OutputLog_s("Razer CreateMouseEffect called.", LogLevel::Debug);
	return 0;
}

RZRESULT CreateHeadsetEffect(ChromaSDK::Headset::EFFECT_TYPE Effect, PRZPARAM pParam, RZEFFECTID* pEffectId)
{
	CLogger::OutputLog_s("Razer CreateHeadsetEffect called.", LogLevel::Debug);
	return 0;
}

RZRESULT CreateMousepadEffect(ChromaSDK::Mousepad::EFFECT_TYPE Effect, PRZPARAM pParam, RZEFFECTID* pEffectId)
{
	CLogger::OutputLog_s("Razer CreateMousepadEffect called.", LogLevel::Debug);
	return 0;
}

RZRESULT CreateKeypadEffect(ChromaSDK::Keypad::EFFECT_TYPE Effect, PRZPARAM pParam, RZEFFECTID* pEffectId)
{
	CLogger::OutputLog_s("Razer CreateKeypadEffect called.", LogLevel::Debug);
	return 0;
}

RZRESULT DeleteEffect(RZEFFECTID EffectId)
{
	CLogger::OutputLog_s("Razer DeleteEffect called.", LogLevel::Debug);
	return 0;
}

RZRESULT SetEffect(RZEFFECTID EffectId)
{
	CLogger::OutputLog_s("Razer SetEffect called.", LogLevel::Debug);
	return 0;
}

RZRESULT RegisterEventNotification(HWND hWnd)
{
	CLogger::OutputLog_s("Razer RegisterEventNotification called.", LogLevel::Debug);
	return 0;
}

RZRESULT UnregisterEventNotification()
{
	CLogger::OutputLog_s("Razer UnregisterEventNotification called.", LogLevel::Debug);
	return 0;
}

RZRESULT QueryDevice(RZDEVICEID DeviceId, DEVICE_INFO_TYPE& DeviceInfo)
{
	CLogger::OutputLog_s("Razer QueryDevice called.", LogLevel::Debug);
	return 0;
}
