using System;

namespace JoksterCube.ServerPlayerList.Commonn;

internal static class ZRoutedRpcExtensions
{
	internal static bool RegisterOnce(this ZRoutedRpc rpc, string name, Action<long> f)
	{
		if (rpc == null || string.IsNullOrEmpty(name) || f == null) return false;
		int hash = name.GetStableHashCode();
		if (rpc.m_functions.ContainsKey(hash)) return false;
		rpc.Register(name, f);
		return true;
	}

	internal static bool RegisterOnce<T1>(this ZRoutedRpc rpc, string name, Action<long, T1> f)
	{
		if (rpc == null || string.IsNullOrEmpty(name) || f == null) return false;
		int hash = name.GetStableHashCode();
		if (rpc.m_functions.ContainsKey(hash)) return false;
		rpc.Register(name, f);
		return true;
	}

	internal static bool RegisterOnce<T1, T2>(this ZRoutedRpc rpc, string name, Action<long, T1, T2> f)
	{
		if (rpc == null || string.IsNullOrEmpty(name) || f == null) return false;
		int hash = name.GetStableHashCode();
		if (rpc.m_functions.ContainsKey(hash)) return false;
		rpc.Register(name, f);
		return true;
	}

	internal static bool RegisterOnce<T1, T2, T3>(this ZRoutedRpc rpc, string name, Action<long, T1, T2, T3> f)
	{
		if (rpc == null || string.IsNullOrEmpty(name) || f == null) return false;
		int hash = name.GetStableHashCode();
		if (rpc.m_functions.ContainsKey(hash)) return false;
		rpc.Register(name, f);
		return true;
	}
}