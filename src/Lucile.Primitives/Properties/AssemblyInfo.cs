using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: AssemblyTrademark("")]
[assembly: ComVisible(false)]
[assembly: Guid("ca770a5c-a95f-4fbf-b3b3-9dccf7877871")]
#if SIGN
[assembly: InternalsVisibleTo("Lucile.Core, PublicKey=00240000048000009400000006020000002400005253413100040000010001000f78ef8ab960561eba66213ff50f134638f5403f43965f1a9e5af4eac1fa8a10589a0acda504b595b7f457871af72dd738c57e3e75af0fc627333f6d354cd52f38b58620cb60bb13f4614e4ed07d90fce12ef102453e9ef7c20e267bd13603d0431591571d28dae151defc4cb587c5444756ec7bb43c3f5a844a02d129c2a7c6")]
#else
[assembly: InternalsVisibleTo("Lucile.Core")]
#endif