// <copyright file="CompetenceRecommendationAsset.cs" company="RAGE">
// Copyright (c) 2016 RAGE All rights reserved.
// </copyright>
// <author>mmaurer</author>
// <date>08.02.2016 09:42:08</date>
// <summary>Implements the CompetenceRecommendationAsset class</summary>
namespace CompetenceRecommendationAssetNameSpace
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using AssetManagerPackage;
    using AssetPackage;

    /// <summary>
    /// An asset.
    /// </summary>
    public class CompetenceRecommendationAsset : BaseAsset
    {
        #region Fields

        /// <summary>
        /// Options for controlling the operation.
        /// </summary>
        private CompetenceRecommendationAssetSettings settings = null;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the CompetenceRecommendationAsset.Asset class.
        /// </summary>
        public CompetenceRecommendationAsset()
            : base()
        {
            //! Create Settings and let it's BaseSettings class assign Defaultvalues where it can.
            // 
            settings = new CompetenceRecommendationAssetSettings();
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
                settings = (value as CompetenceRecommendationAssetSettings);
            }
        }

        #endregion Properties

        #region Methods

        // Your code goes here.
        /*
public void test()
{
    Console.WriteLine("CompetenceRecommendation method called!");
    CompetenceRecommendationHandler.Instance.performAllTests();
}
*/

        /// <summary>
        /// Method returning the next game situation id for the player.
        /// </summary>
        /// 
        /// <param name="playerId"> Player identification. </param>
        /// 
        /// <returns> The game situation id for the player. </returns>
        public string getNextGameSituationId(string playerId)
        {
            if (CompetenceRecommendationHandler.Instance.getCurrentGameSituationId(playerId) == null)
            {
                CompetenceRecommendationHandler.Instance.registerNewPlayer(playerId, DomainModelHandler.Instance.getDomainModel(playerId));
                return CompetenceRecommendationHandler.Instance.getCurrentGameSituationId(playerId);
            }
            return CompetenceRecommendationHandler.Instance.getNextGameSituationId(playerId);
        }

        /// <summary>
        /// Method returning the current game situation id for the player.
        /// </summary>
        /// 
        /// <param name="playerId"> Player identification. </param>
        /// 
        /// <returns> The game situation id for the player. </returns>
        public string getCurrentGameSituationId(string playerId)
        {
            if (CompetenceRecommendationHandler.Instance.getCurrentGameSituationId(playerId) == null)
                CompetenceRecommendationHandler.Instance.registerNewPlayer(playerId, DomainModelHandler.Instance.getDomainModel(playerId));
            return CompetenceRecommendationHandler.Instance.getCurrentGameSituationId(playerId);
        }

        /// <summary>
        /// Method for updating the competence state of a player due to performance in a game situation.
        /// </summary>
        /// 
        /// <param name="playerId"> Player identification. </param>
        /// <param name="type"> If true, the player successfully played the curren game situation, otherwise not. </param>
        public void setGameSituationUpdate(string playerId, Boolean type)
        {
            if (CompetenceRecommendationHandler.Instance.getCurrentGameSituationId(playerId) == null)
                CompetenceRecommendationHandler.Instance.registerNewPlayer(playerId, DomainModelHandler.Instance.getDomainModel(playerId));

            CompetenceRecommendationHandler.Instance.setGameSituationUpdate(playerId, type);
        }

        #endregion Methods
    }
}