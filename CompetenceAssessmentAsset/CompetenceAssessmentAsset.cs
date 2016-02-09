// <copyright file="CompetenceAssessmentAsset.cs" company="RAGE">
// Copyright (c) 2016 RAGE All rights reserved.
// </copyright>
// <author>mmaurer</author>
// <date>08.02.2016 09:41:04</date>
// <summary>Implements the CompetenceAssessmentAsset class</summary>
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

        /*
        public void test()
        {
            Console.WriteLine("CompetenceAssessment methode called!");
            //CompetenceAssessmentHandler.Instance.performAllTests();
        }
        */

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
        public void registerNewPlayer(string playerId, DomainModel dm)
        {
            CompetenceAssessmentHandler.Instance.registerNewPlayer(playerId, dm);
        }

        #endregion Methods
    }
}