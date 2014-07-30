////////////////////////////////////////////////////////////////////////////////
/// Winamp C# Plugin Wrapper - Part of Sharpamp C# library
////////////////////////////////////////////////////////////////////////////////
#pragma once
using namespace System;

ref class PluginWrapper
{
public:
	// The plugin itself
	static Daniel15::Sharpamp::GeneralPlugin^ plugin = gcnew Winamp::Wcf::Plugin::WinampWcfPlugin();
	// Name of the plugin
	static char * Name();
};
