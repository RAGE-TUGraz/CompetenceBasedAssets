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

namespace CompetenceAssessmentAssetNameSpace
{
    using System;
    using System.Collections.Generic;

    using AssetManagerPackage;
    using AssetPackage;
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
                CompetenceAssessmentHandler.Instance.transitionProbability = settings.TransitionProbability;
            }
        }

        #endregion Properties
        #region Methods
        
        /// <summary>
        /// Method for updating the competence state of a player.
        /// </summary>
        /// 
        /// <param name="competences"> Id of a competence for which an evidence is observed. </param>
        /// <param name="evidences"> If true the evidence indicates possession of the specified competence, otherwise a lack of this competence is indicated. </param>
        /// <param name="evidencePowers"> Contains the power of the evidence (Low,Medium,High) </param>
        public void updateCompetenceState(List<string> competences, List<Boolean> evidences, List<EvidencePower> evidencePowers)
        {
            if (CompetenceAssessmentHandler.Instance.getCompetenceState() == null || CompetenceAssessmentHandler.Instance.updateLevelStorage == null)
                CompetenceAssessmentHandler.Instance.registerNewPlayer( CompetenceAssessmentHandler.Instance.getDMA().getDomainModel());
            CompetenceAssessmentHandler.Instance.updateCompetenceState(competences, evidences, evidencePowers);
        }

        /// <summary>
        /// Method for updating the competence based on observed in-game activities
        /// </summary>
        /// <param name="activity"> observed activity </param>
        public void updateCompetenceStateAccordingToActivity(String activity)
        {
            if (CompetenceAssessmentHandler.Instance.getCompetenceState() == null || CompetenceAssessmentHandler.Instance.updateLevelStorage == null)
                CompetenceAssessmentHandler.Instance.registerNewPlayer(CompetenceAssessmentHandler.Instance.getDMA().getDomainModel());
            CompetenceAssessmentHandler.Instance.activityMapping.updateCompetenceAccordingToActivity(activity);
        }

        /// <summary>
        /// Updates the competence state according to the current game situation and information about sucess/failure
        /// </summary>
        /// <param name="gamesituationId"> id of the gamesituation </param>
        /// <param name="success"> true, if the gamesituation was mastered, false otherwise</param>
        public void updateCompetenceStateAccordingToGamesituation(String gamesituationId, Boolean success)
        {
            if (CompetenceAssessmentHandler.Instance.getCompetenceState() == null || CompetenceAssessmentHandler.Instance.updateLevelStorage == null)
                CompetenceAssessmentHandler.Instance.registerNewPlayer(CompetenceAssessmentHandler.Instance.getDMA().getDomainModel());
            CompetenceAssessmentHandler.Instance.gameSituationMapping.updateCompetenceAccordingToGamesituation(gamesituationId,success);
        }

        /// <summary>
        /// Method returning the current competence state of a player.
        /// </summary>
        /// 
        /// <returns> Competence state</returns>
        public Dictionary<string, double> getCompetenceState()
        {
            if (CompetenceAssessmentHandler.Instance.getCompetenceState() == null || CompetenceAssessmentHandler.Instance.updateLevelStorage == null)
                CompetenceAssessmentHandler.Instance.registerNewPlayer(CompetenceAssessmentHandler.Instance.getDMA().getDomainModel());

            if (CompetenceAssessmentHandler.Instance.gameStorage == null)
                CompetenceAssessmentHandler.Instance.loadCompetenceStateFromGameStorage();

            Dictionary<Competence, double> cs = CompetenceAssessmentHandler.Instance.getCompetenceState().getCurrentValues();
            Dictionary<string, double> csNew = new Dictionary<string, double>();
            foreach (KeyValuePair<Competence, double> pair in cs)
                csNew[pair.Key.id] = pair.Value;
            return csNew;
        }

        /// <summary>
        /// Method for resetting the current competence state to the starting competence state
        /// </summary>
        public void resetCompetenceState()
        {
            if (CompetenceAssessmentHandler.Instance.getCompetenceState() == null || CompetenceAssessmentHandler.Instance.updateLevelStorage == null)
                CompetenceAssessmentHandler.Instance.registerNewPlayer(CompetenceAssessmentHandler.Instance.getDMA().getDomainModel());
            CompetenceAssessmentHandler.Instance.resetCompetenceState();
        }

        #endregion Methods
    }
}