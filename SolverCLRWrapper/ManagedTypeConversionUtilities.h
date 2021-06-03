#pragma once

using namespace System::Runtime::InteropServices;

static const char* string_to_char_array(System::String^ string)
{
    const char* str = (const char*)(Marshal::StringToHGlobalAnsi(string)).ToPointer();
    return str;
}