using System;
using System.Collections.Generic;
using System.Text;

namespace SFBF
{
    /// <summary>
    /// Class responsible for managing AssetSets
    /// </summary>
    public sealed class AssetManager
    {
        private Type AssetSetType;
        public AssetBox Assets;

        public void UnloadAll()
        {
            Assets = (AssetBox)Activator.CreateInstance(AssetSetType);
        }

        internal class AssetManagerController
        {
            public AssetManager assetMgr = new AssetManager();

            public void SetAssetBoxType(Type type)
            {
                assetMgr.AssetSetType = type;
                assetMgr.UnloadAll();
            }
        }
    }
}
