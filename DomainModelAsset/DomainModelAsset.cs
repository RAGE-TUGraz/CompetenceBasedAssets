/*
  Copyright 2016 TUGraz, http://www.tugraz.at/
  
  Licensed under the Apache License, Version 2.0 (the "License");
  you may not use this file except in compliance with the License.
  This project has received funding from the European Union�s Horizon
  2020 research and innovation programme under grant agreement No 644187.
  You may obtain a copy of the License at
  
      http://www.apache.org/licenses/LICENSE-2.0
  
  Unless required by applicable law or agreed to in writing, software
  distributed under the License is distributed on an "AS IS" BASIS,
  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
  See the License for the specific language governing permissions and
  limitations under the License.
  
  This software has been created in the context of the EU-funded RAGE project.
  Realising and Applied Gaming Eco-System (RAGE), Grant agreement No 644187, 
  http://rageproject.eu/

  Development was done by Cognitive Science Section (CSS) 
  at Knowledge Technologies Institute (KTI)at Graz University of Technology (TUGraz).
  http://kti.tugraz.at/css/

  Created by: Matthias Maurer, TUGraz <mmaurer@tugraz.at>
*/

namespace DomainModelAssetNameSpace
{
    using AssetPackage;

    /// <summary>
    /// An asset.
    /// </summary>
    public class DomainModelAsset : BaseAsset
    {
        #region Fields

        /// <summary>
        /// Options for controlling the operation.
        /// </summary>
        private DomainModelAssetSettings settings = null;


        /// <summary>
        /// Instance of the class DomainModelHandler - Singelton pattern
        /// </summary>
        static readonly DomainModelAsset instance = new DomainModelAsset();

        /// <summary>
        /// Instance of the class DomainModelHandler - Singelton pattern
        /// </summary>
        static internal DomainModelHandler domainModelHandler = new DomainModelHandler();

        #endregion Fields
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the DomainModelAsset.Asset class.
        /// </summary>
        private DomainModelAsset()
            : base()
        {
            //! Create Settings and let it's BaseSettings class assign Defaultvalues where it can.
            settings = new DomainModelAssetSettings();
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
                Handler.setDomainModel(null);
            }
        }

        /// <summary>
        /// DomainModel Property
        /// </summary>
        public DomainModel DomainModel
        {
            get
            {
                return Handler.DomainModel;
            }
        }
        
        /// <summary>
        /// Getter for Instance of the DomainModelAsset - Singelton pattern
        /// </summary>
        public static DomainModelAsset Instance
        {
            get
            {
                return instance;
            }
        }

        /// <summary>
        /// Getter for Instance of the DomainModelHandler
        /// </summary>
        internal static DomainModelHandler Handler
        {
            get
            {
                return domainModelHandler;
            }
        }

        #endregion Properties
        #region PublicMethods
        
        /// <summary>
        /// Method returning domain model either from the run-time asset storage if available or from specified (default) source(File/Web).
        /// </summary>
        /// 
        /// <param name="playerId"> Id of the player for which the domain model is requested. </param>
        /// 
        /// <returns> The domein model associated with the player-id. </returns>
        public DomainModel getDomainModel()
        {
            return Handler.DomainModel;
        }

        #endregion PublicMethods
        #region internal Methods

        /// <summary>
        /// Wrapper method for getting the getInterface method of the base Asset
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <returns>Corresponding Interface</returns>
        internal T getInterfaceFromAsset<T>()
        {
            return this.getInterface<T>();
        }

        #endregion internal Methods
    }
}
