using NUnit.Framework;
using O2un.Manager;

namespace O2un.CommonFramework.Tests
{
    public sealed class AssemblyResolveTests
    {
        [Test]
        public void CommonFrameworkTypesAreVisibleFromTestAssembly()
        {
            Assert.IsNotNull(typeof(AssetManager));
        }
    }
}
