/*
  Copyright 2016 TUGraz, http://www.tugraz.at/
  
  Licensed under the Apache License, Version 2.0 (the "License");
  you may not use this file except in compliance with the License.
  This project has received funding from the European Union’s Horizon
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

namespace CompetenceBasedAdaptionAssetNameSpace
{
    using System;
    using AssetPackage;
    /// <summary>
    /// An asset.
    /// </summary>
    public class CompetenceBasedAdaptionAsset : BaseAsset
    {
        #region Fields

        /// <summary>
        /// Options for controlling the operation.
        /// </summary>
        private CompetenceBasedAdaptionAssetSettings settings = null;


        /// <summary>
        /// Instance of the CompetenceBasedAdaptionAsset - Singelton pattern
        /// </summary>
        static readonly CompetenceBasedAdaptionAsset instance = new CompetenceBasedAdaptionAsset();


        /// <summary>
        /// Instance of the CompetenceRecommendationHandler 
        /// </summary>
        static internal CompetenceBasedAdaptionHandler competenceBasedAdaptionHandler = new CompetenceBasedAdaptionHandler();

        #endregion Fields
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the CompetenceRecommendationAsset.Asset class.
        /// </summary>
        private CompetenceBasedAdaptionAsset()
            : base()
        {
            //! Create Settings and let it's BaseSettings class assign Defaultvalues where it can.
            settings = new CompetenceBasedAdaptionAssetSettings();
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
                settings = (value as CompetenceBasedAdaptionAssetSettings);
                CompetenceBasedAdaptionAsset.Handler.resetAsset();
            }
        }

        /// <summary>
        /// Getter for Instance of the CompetenceBasedAdaptionAsset - Singelton pattern
        /// </summary>
        public static CompetenceBasedAdaptionAsset Instance
        {
            get
            {
                return instance;
            }
        }

        /// <summary>
        /// Getter for Instance of the CompetenceBasedAdaptionHandler
        /// </summary>
        internal static CompetenceBasedAdaptionHandler Handler
        {
            get
            {
                return competenceBasedAdaptionHandler;
            }
        }
        
        #endregion Properties
        #region Methods

        /// <summary>
        /// Method returning the next game situation id for the player.
        /// </summary>
        /// 
        /// <returns> The game situation id for the player. </returns>
        public string getNextGameSituationId()
        {
            if (Handler.getCurrentGameSituationId() == null)
            {
                Handler.registerNewPlayer(Handler.getDMA().getDomainModel());
                return Handler.getNextGameSituationId();
            }
            return Handler.getNextGameSituationId();
        }

        /// <summary>
        /// Method returning the current game situation id for the player.
        /// </summary>
        /// 
        /// <returns> The game situation id for the player. </returns>
        public string getCurrentGameSituationId()
        {
            if (Handler.getCurrentGameSituationId() == null)
                Handler.registerNewPlayer(Handler.getDMA().getDomainModel());
            return Handler.getCurrentGameSituationId();
        }
        
        /// <summary>
        /// Method for updating the competence state of the player due to performance in a game situation.
        /// </summary>
        /// 
        /// <param name="evidence"> If true, the player successfully played the curren game situation, otherwise not. </param>
        public void setGameSituationUpdate( Boolean evidence)
        {
            if (Handler.getCurrentGameSituationId() == null)
                Handler.registerNewPlayer(Handler.getDMA().getDomainModel());

            Handler.setGameSituationUpdate(evidence);
        }

        #endregion Methods
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