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
  Changed by: Matthias Maurer, TUGraz <mmaurer@tugraz.at>
  Changed on: 2016-02-22
*/

namespace CompetenceAssessmentAssetNameSpace
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using AssetManagerPackage;
    using AssetPackage;
    using DomainModelAssetNameSpace;
    /// <summary>
    /// An asset.
    /// </summary>
    public class CompetenceAssessmentAsset : BaseAsset
    {
        #region Fields

        /// <summary>
        /// Options for controlling the operation.
        /// </summary>
        private CompetenceAssessmentAssetSettings settings = null;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the CompetenceAssessmentAsset.Asset class.
        /// </summary>
        public CompetenceAssessmentAsset()
            : base()
        {
            //! Create Settings and let it's BaseSettings class assign Defaultvalues where it can.
            // 
            settings = new CompetenceAssessmentAssetSettings();

            //preventing multiple asset creation
            if (AssetManager.Instance.findAssetsByClass(this.Class).Count > 1)
            {
                this.Log(Severity.Error, "There is only one instance of the CompetenceAssessmentAsset permitted!");
                throw new Exception("EXCEPTION: There is only one instance of the CompetenceAssessmentAsset permitted!");
            }

            //control if an instance of the DomainModelAsset exists
            if (AssetManager.Instance.findAssetsByClass("DomainModelAsset").Count == 0)
            {
                this.Log(Severity.Error, "There needs to be an instance of the DomainModelAsset persistent before creating the CompetenceAssessmentAsset!");
                throw new Exception("EXCEPTION: There needs to be an instance of the DomainModelAsset persistent before creating the CompetenceAssessmentAsset!");
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
                settings = (value as CompetenceAssessmentAssetSettings);
            }
        }

        #endregion Properties

        #region Methods

        // Your code goes here.

        /// <summary>
        /// Method for internal testing
        /// </summary>
        public void performTests()
        {
            //Console.WriteLine("CompetenceAssessment methode called!");
            CompetenceAssessmentHandler.Instance.performAllTests();
        }
        

        /// <summary>
        /// Method for updating the competence state of a player.
        /// </summary>
        /// 
        /// <param name="playerId"> Player Id for the update. </param>
        /// <param name="evidence"> Id of a competence for which an evidence is observed. </param>
        /// <param name="type"> If true the evidence indicates possession of the specified competence, otherwise a lack of this competence is indicated. </param>
        public void updateCompetenceState(string playerId, List<string> evidences, List<Boolean> type)
        {
            if (CompetenceAssessmentHandler.Instance.getCompetenceState(playerId) == null)
                CompetenceAssessmentHandler.Instance.registerNewPlayer(playerId, CompetenceAssessmentHandler.Instance.getDMA().getDomainModel(playerId));
            CompetenceAssessmentHandler.Instance.updateCompetenceState(playerId, evidences, type);
        }

        /// <summary>
        /// Method returning the current competence state of a player.
        /// </summary>
        /// 
        /// <param name="playerId"> Player identification </param>
        /// <returns></returns>
        public Dictionary<string, double> getCompetenceState(string playerId)
        {
            if (CompetenceAssessmentHandler.Instance.getCompetenceState(playerId) == null)
                CompetenceAssessmentHandler.Instance.registerNewPlayer(playerId, CompetenceAssessmentHandler.Instance.getDMA().getDomainModel(playerId));
            Dictionary<Competence, double> cs = CompetenceAssessmentHandler.Instance.getCompetenceState(playerId).getCurrentValues();
            Dictionary<string, double> csNew = new Dictionary<string, double>();
            foreach (KeyValuePair<Competence, double> pair in cs)
                csNew[pair.Key.id] = pair.Value;
            return csNew;
        }

        /// <summary>
        /// Method for performing all neccessary operations to run update methods.
        /// </summary>
        /// 
        /// <param name="playerId"> Player Id which is created. </param>
        /// <param name="dm"> Specifies the domain model used for the following registration. </param>
        private void registerNewPlayer(string playerId, DomainModel dm)
        {
            CompetenceAssessmentHandler.Instance.registerNewPlayer(playerId, dm);
        }

        #endregion Methods
    }
}