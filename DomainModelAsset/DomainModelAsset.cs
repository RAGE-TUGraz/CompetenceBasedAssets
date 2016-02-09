// <copyright file="DomainModelAsset.cs" company="RAGE">
// Copyright (c) 2016 RAGE All rights reserved.
// </copyright>
// <author>mmaurer</author>
// <date>08.02.2016 09:42:23</date>
// <summary>Implements the DomainModelAsset class</summary>
namespace DomainModelAssetNameSpace
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using AssetManagerPackage;
    using AssetPackage;

    /// <summary>
    /// An asset.
    /// </summary>
    public class DomainModelAsset : BaseAsset
    {
        public string msg = "OK";

        #region Fields

        /// <summary>
        /// Options for controlling the operation.
        /// </summary>
        private DomainModelAssetSettings settings = null;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the DomainModelAsset.Asset class.
        /// </summary>
        public DomainModelAsset()
            : base()
        {
            //! Create Settings and let it's BaseSettings class assign Defaultvalues where it can.
            // 
            settings = new DomainModelAssetSettings();

            //preventing multiple asset creation
            if (AssetManager.Instance.findAssetsByClass(this.Class).Count > 1)
            {
                this.Log(Severity.Error, "There is only one instance of the DomainModelAsset permitted!");
                throw new Exception("EXCEPTION: There is only one instance of the DomainModelAsset permitted!");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets options for controlling the operation.
        /// </summary>
        ///
        /// <remarks>   Besides the toXml() and fromXml() methods, we never use this property but use
        ///                it's correctly typed backing field 'settings' instead. </remarks>
        /// <remarks> This property should go into each asset having Settings of its own. </remarks>
        /// <remarks>   The actual class used should be derived from BaseAsset (and not directly from
        ///             ISetting). </remarks>
        ///
        /// <value>
        /// The settings.
        /// </value>
        public override ISettings Settings
        {
            get
            {
                return settings;
            }
            set
            {
                settings = (value as DomainModelAssetSettings);
            }
        }

        #endregion Properties

        #region Methods

        // Your code goes here.
        /*
        public void test()
        {
            Console.WriteLine("DomainModel method called!");
            DomainModelHandler.Instance.performAllTests();
        }
        */
        /*
        public String getDM()
        {
            String url = @"http://css-kmi.tugraz.at:8080/compod/rest/getdomainmodel?id=isr2013";
            return DomainModelHandler.Instance.getDMFromWeb(url).toXmlString();
        }
        */

        /// <summary>
        /// tmp. solution: sets the default loading location for a xml file. 
        /// </summary>
        /// 
        /// <param name="filepath"> Path to the xml-file. If the value is "" an example domain model is created. </param>
        public void setLocalFileAsDefaultDmSource(string filepath)
        {
            DomainModelHandler.Instance.setDmPath(filepath);
        }

        /// <summary>
        /// Method returning domain model either from the run-tima asset storage if available or from specified (default) source(File/Web).
        /// </summary>
        /// 
        /// <param name="playerId"> Id of the player for which the domain model is requested. </param>
        /// 
        /// <returns> The domein model associated with the player-id. </returns>
        public DomainModel getDomainModel(String playerId)
        {
            return DomainModelHandler.Instance.getDomainModel(playerId);
        }

        #endregion Methods
    }
}