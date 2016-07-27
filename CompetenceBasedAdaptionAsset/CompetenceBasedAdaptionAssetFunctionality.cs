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

using AssetManagerPackage;
using AssetPackage;
using CompetenceAssessmentAssetNameSpace;
using DomainModelAssetNameSpace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CompetenceBasedAdaptionAssetNameSpace
{

    /// <summary>
    /// Singelton Class for handling Competence based adaption
    /// </summary>
    internal class CompetenceBasedAdaptionHandler
    {
        #region Fields

        /// <summary>
        /// Instance of the DomainModelAsset
        /// </summary>
        private DomainModelAsset domainModelAsset = null;

        /// <summary>
        /// Instance of the CompetenceAssessmentAsset
        /// </summary>
        private CompetenceAssessmentAsset competenceAssessmentAsset = null;

        /// <summary>
        /// Instance of the CompetenceBasedAdaptionHandler
        /// </summary>
        private CompetenceBasedAdaptionAsset competenceBasedAdaptionAsset = null;

        /// <summary>
        /// Instance of the CompetenceRecommendationHandler - Singelton pattern
        /// </summary>
        private static CompetenceBasedAdaptionHandler instance;

        /// <summary>
        /// Dictionary storing all current game situation with player id as key.
        /// </summary>
        private GameSituation currentGameSituation = null;

        //TODO: List of players -> game situation structure?
        //storage gss?
        /// <summary>
        /// Dictionary storing all game situation structures with player id as key.
        /// </summary>
        private GameSituationStructure gameSituationStructure = null;

        /// <summary>
        /// Storage of player id and game situation counter - how often has player played the game situations
        /// </summary>
        private Dictionary<GameSituation, int> gameSituationHistory = new Dictionary<GameSituation, int>();

        /// <summary>
        /// If true logging is done, otherwise no logging is done.
        /// </summary>
        private Boolean doLogging = true;

        #endregion Fields
        #region Constructors

        /// <summary>
        /// Private ctor - Singelton pattern
        /// </summary>
        private CompetenceBasedAdaptionHandler() { }

        #endregion Constructors
        #region Properties

        /// <summary>
        /// Getter for Instance of the CompetenceBasedAdaptionHandler - Singelton pattern
        /// </summary>
        public static CompetenceBasedAdaptionHandler Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new CompetenceBasedAdaptionHandler();
                }
                return instance;
            }
        }

        /// <summary>
        /// If set to true - logging is done, otherwise no logging is done.
        /// </summary>
        public Boolean DoLogging
        {
            set
            {
                doLogging = value;
            }
            get
            {
                return doLogging;
            }
        }

        #endregion properties
        #region InternalMethods

        /// <summary>
        /// Method for resetting the Asset - if for example the settigns are changed
        /// </summary>
        public void resetAsset()
        {
            this.currentGameSituation = null;
        }

        /// <summary>
        /// Method returning an instance of the DomainModelAsset.
        /// </summary>
        /// <returns> Instance of the DomainModelAsset </returns>
        internal DomainModelAsset getDMA()
        {
            if (domainModelAsset == null)
                domainModelAsset = (DomainModelAsset)AssetManager.Instance.findAssetByClass("DomainModelAsset");
            return (domainModelAsset);
        }

        /// <summary>
        /// Method returning an instance of the DomainModelAsset.
        /// </summary>
        /// <returns> Instance of the CompetenceAssessmentAsset </returns>
        internal CompetenceAssessmentAsset getCAA()
        {
            if (competenceAssessmentAsset == null)
                competenceAssessmentAsset = (CompetenceAssessmentAsset)AssetManager.Instance.findAssetByClass("CompetenceAssessmentAsset");
            return (competenceAssessmentAsset);
        }
        
        /// <summary>
        /// Method returning the current game situation of an player by playerId.
        /// </summary>
        /// 
        /// <returns> GameSituation the player is currently in. </returns>
        internal GameSituation getCurrentGameSituation()
        {
            if (currentGameSituation == null)
            {
                loggingPRA("Player is not associated with a current GameSituation!");
                return null;
            }

            return currentGameSituation;
        }

        /// <summary>
        /// Stores a game situation to a given player id as current game situation.
        /// </summary>
        /// 
        /// <param name="gs"> Game situation which is set to be the current GS for the specified player. </param>
        internal void setCurrentGameSituation(GameSituation gs)
        {
            currentGameSituation = gs;
        }

        /// <summary>
        /// Method returning the game situation structure of the player.
        /// </summary>
        /// 
        /// <returns>Game situation structure for the player. </returns>
        internal GameSituationStructure getGameSituationStructure()
        {
            if (gameSituationStructure == null)
            {
                loggingPRA("Player is not associated with a GameSituationStructure!");
                return null;
            }

            return gameSituationStructure;
        }

        /// <summary>
        /// Sets a game situation structure to a player.
        /// </summary>
        /// 
        /// <param name="gss"> Game situation structure which gets linked to the player id. </param>
        internal void setGameSituationStructure( GameSituationStructure gss)
        {
            gameSituationStructure = gss;
        }

        /// <summary>
        /// Returns the game situation history of the player.
        /// </summary>
        /// 
        /// <returns> A dictionary containing the game situations as keys and the number of times they where player by the player as values. </returns>
        internal Dictionary<GameSituation, int> getGameSituationHistory()
        {
            if (gameSituationHistory == null)
                gameSituationHistory = new Dictionary<GameSituation, int>();

            if(gameSituationHistory.Count != this.gameSituationStructure.GameSituations.Count)
            {
                gameSituationHistory = new Dictionary<GameSituation, int>();
                foreach (GameSituation gs in gameSituationStructure.GameSituations)
                    gameSituationHistory.Add(gs,0);
            }

            return gameSituationHistory;
        }

        /// <summary>
        /// Increments the integer counting the number of times a player has player a game situation.
        /// </summary>
        /// 
        /// <param name="gs"> Game situation played. </param>
        internal void updateGameSituationHistory(GameSituation gs)
        {
            gameSituationHistory[gs]++;
        }

        #endregion InternalMethods
        #region PublicMethods

        /// <summary>
        /// Returns the Id of the next game situation.
        /// </summary>
        /// 
        /// 
        /// <returns> Identification of the next game situation proposed for the player. </returns>
        public String getNextGameSituationId( )
        {
            if (gameSituationStructure == null)
            {
                loggingPRA("The game situation structure for the player does not exist.");
                return null;
            }
            GameSituationStructure gss = gameSituationStructure;
            GameSituation nextGS = gss.determineNextGameSituation();
            if (nextGS != null)
            {
                updateGameSituationHistory(nextGS);
                return nextGS.Id;
            }
            return null;
        }

        /// <summary>
        /// Method returning the id of the current game situation player by the player.
        /// </summary>
        /// 
        /// <returns> String containing the game situation identification. </returns>
        public String getCurrentGameSituationId()
        {
            GameSituation gs = getCurrentGameSituation();
            if (gs == null)
                return null;
            return gs.Id;
        }

        /// <summary>
        /// Method updating the competence state due to a game situation success/failure.
        /// </summary>
        /// 
        /// <param name="success"> If this value is set to true the player has successfully completed the game situation, otherwise not. </param>
        public void setGameSituationUpdate( Boolean success)
        {
            loggingPRA("Gamesituation completed - sending evidence to update competences.");
            GameSituation gs = getCurrentGameSituation();
            getCAA().updateCompetenceStateAccordingToGamesituation(gs.Id,success);
        }

        /// <summary>
        /// Method for performing all neccessary operations to run update methods.
        /// </summary>
        /// 
        /// <param name="dm"> Specifies the domain model used for the following registration. </param>
        public void registerNewPlayer( DomainModel dm)
        {
            GameSituationStructure gss = new GameSituationStructure(dm);
            setGameSituationStructure( gss);
            setCurrentGameSituation( gss.InitialGameSituation);
        }

        #endregion PublicMethods
        #region TestMethods

        /// <summary>
        /// Diagnostic logging method.
        /// </summary>
        /// 
        /// <param name="msg"> String to be logged.  </param>
        /// <param name="severity"> Severity of the logging-message, optional. </param>
        internal void loggingPRA(String msg, Severity severity = Severity.Information)
        {
            if (DoLogging)
            {
                if (competenceBasedAdaptionAsset == null)
                    competenceBasedAdaptionAsset = (CompetenceBasedAdaptionAsset)AssetManager.Instance.findAssetByClass("CompetenceBasedAdaptionAsset");
                competenceBasedAdaptionAsset.Log(severity, "[CBAA]: " + msg);
            }
        }

        #endregion TestMethod
    }

    /// <summary>
    /// Class describing a game situation.
    /// </summary>
    public class GameSituation
    {
        #region Fields

        /// <summary>
        /// Identification of the game situation.
        /// </summary>
        private String id;

        /// <summary>
        /// List containing all competences (as String) needed to master this game situation.
        /// </summary>
        private List<String> competences = new List<String>();

        /// <summary>
        /// List of all possible successors to this game situation based on story-path.
        /// </summary>
        private List<GameSituation> successors = new List<GameSituation>();

        #endregion Fields
        #region Constructors

        /// <summary>
        /// C-tor for game situation.
        /// </summary>
        /// 
        /// <param name="id"> Game situation identifier. </param>
        public GameSituation(String id)
        {
            this.id = id;
        }

        #endregion Constructors
        #region Properties

        /// <summary>
        /// List containing all competences (as String) needed to master this game situation.
        /// </summary>
        public List<String> Competences
        {
            get
            {
                return competences;
            }
            set
            {
                competences = value;
            }
        }

        /// <summary>
        /// Identification of the game situation.
        /// </summary>
        public String Id
        {
            get
            {
                return id;
            }
            set
            {
                id = value;
            }
        }

        /// <summary>
        /// List of all possible successors to this game situation based on story-path.
        /// </summary>
        public List<GameSituation> Successors
        {
            get
            {
                return successors;
            }
            set
            {
                successors = value;
            }
        }

        #endregion Properties
        #region Methods

        /// <summary>
        /// Returns the number of competences needed to play this game situation without already mastered competences.
        /// </summary>
        /// 
        /// <param name="masteredCompetences"> String-list of already mastered competences. </param>
        /// 
        /// <returns> Number of competences not mastered and needed for this game situation. </returns>
        internal int getNrNotMasteredCompetences(List<String> masteredCompetences)
        {
            int number = 0;
            foreach (String com in competences)
                if (!masteredCompetences.Contains(com))
                    number++;

            return number;
        }

        /// <summary>
        /// Prints out the GS.
        /// </summary>
        internal void diagnosticPrint()
        {
            CompetenceBasedAdaptionHandler.Instance.loggingPRA(id + "          " + " competences: " + string.Join(",", competences.ToArray()));
            String successorsString = "";
            foreach (GameSituation gs in successors)
                successorsString += gs.Id + ",";
            if (successorsString.Length > 0)
                successorsString = successorsString.Remove(successorsString.Length - 1);
            CompetenceBasedAdaptionHandler.Instance.loggingPRA("             " + " successors:  " + successorsString);
        }

        #endregion Methods
    }

    //TODO: there are no restriction of succesor-gs due to story telling implemented
    //TODO: set initial gs
    /// <summary>
    /// Class describing non-functional game situation dependencies.
    /// </summary>
    public class GameSituationStructure
    {
        #region Fields

        /// <summary>
        /// List containing all Gamesituations for a player.
        /// </summary>
        private List<GameSituation> gameSituations = new List<GameSituation>();

        /// <summary>
        /// Gamesituation starting the game.
        /// </summary>
        private GameSituation initialGameSituation;

        #endregion Fields
        #region Constructors

        /// <summary>
        /// default c-tor
        /// </summary>
        public GameSituationStructure() { }

        /// <summary>
        /// c-tor creating the object out of a given domain model.
        /// </summary>
        /// <param name="dm"></param>
        public GameSituationStructure(DomainModel dm)
        {
            //adding gs to the structure
            foreach (Situation si in dm.elements.situations.situationList)
                gameSituations.Add(new GameSituation(si.id));
            //adding competences to the gs in the structure
            foreach (SituationRelation sir in dm.relations.situations.situations)
                foreach(CompetenceSituation cs in sir.competences)
                    getGameSituationById(sir.id).Competences.Add(cs.id);
            //adding successors of the gs - at the moment all gs are successors of all gs
            foreach (GameSituation gs1 in gameSituations)
                foreach (GameSituation gs2 in gameSituations)
                    gs1.Successors.Add(gs2);

            //set initial gs
            initialGameSituation = gameSituations[0];
        }

        #endregion Constructors
        #region Properties

        /// <summary>
        /// List containing all Gamesituations for a player.
        /// </summary>
        public List<GameSituation> GameSituations
        {
            get
            {
                return gameSituations;
            }
            set
            {
                gameSituations = value;
            }
        }

        /// <summary>
        /// Gamesituation starting the game.
        /// </summary>
        public GameSituation InitialGameSituation
        {
            get
            {
                return initialGameSituation;
            }
            set
            {
                initialGameSituation = value;
            }
        }

        #endregion Properties
        #region Methods

        /// <summary>
        /// Method returning the game situation with a given id.
        /// </summary>
        /// <param name="id"> Id of game situation to return. </param>
        /// <returns></returns>
        internal GameSituation getGameSituationById(String id)
        {
            foreach (GameSituation gs in gameSituations)
                if (gs.Id.Equals(id))
                    return gs;

            CompetenceBasedAdaptionHandler.Instance.loggingPRA("ERROR: No game situation with id " + id + " in the game situation structure found!");
            return null;
        }

        //TODO: change tmp line
        /// <summary>
        /// Returns the next game situation for a player.
        /// </summary>
        /// 
        /// 
        /// <returns> The next game situation for the specified player. </returns>
        internal GameSituation determineNextGameSituation( )
        {
            CompetenceBasedAdaptionHandler.Instance.loggingPRA("determining next game situation for player " );
            GameSituation currentGS = CompetenceBasedAdaptionHandler.Instance.getCurrentGameSituation();
            ///tmp line
            //OLD:
            //CompetenceState cs = CompetenceAssessmentHandler.Instance.getCompetenceState(playerId);
            //List<String> mastered = cs.getMasteredCompetencesString();
            //NEW:
            Dictionary<String, double> cs = CompetenceBasedAdaptionHandler.Instance.getCAA().getCompetenceState();
            List<String> mastered = new List<string>();
            foreach(KeyValuePair<String,double> pair in cs)
            {
                if (pair.Value >= 0.75)
                    mastered.Add(pair.Key);
            }


            //each GS gets evaluated: int describes the number of competences not mastered and in the new game situation
            Dictionary<GameSituation, int> gameSituationEvaluation = new Dictionary<GameSituation, int>();

            int minDistanceCompetences = -1;
            int currentDistanceCompetences;

            foreach (GameSituation gs in currentGS.Successors)
            {
                currentDistanceCompetences = gs.getNrNotMasteredCompetences(mastered);
                gameSituationEvaluation[gs] = currentDistanceCompetences;
                if (currentDistanceCompetences != 0 && (minDistanceCompetences == -1 || minDistanceCompetences > currentDistanceCompetences))
                    minDistanceCompetences = currentDistanceCompetences;
            }

            //Determining the GS with the smallest distance which was played least often
            Dictionary<GameSituation, int> gameSituationHistory = CompetenceBasedAdaptionHandler.Instance.getGameSituationHistory();
            List<GameSituation> minDistanceGS = new List<GameSituation>();
            GameSituation minPlayedGS = null;

            foreach (KeyValuePair<GameSituation, int> entry in gameSituationEvaluation)
            {
                if (entry.Value == minDistanceCompetences)
                {
                    minDistanceGS.Add(entry.Key);
                    if (minPlayedGS == null || gameSituationHistory[minPlayedGS] > gameSituationHistory[entry.Key])
                        minPlayedGS = entry.Key;

                }
            }

            CompetenceBasedAdaptionHandler.Instance.setCurrentGameSituation(minPlayedGS);

            return minPlayedGS;
        }

        /// <summary>
        /// Prints out the GSS.
        /// </summary>
        public void diagnosticPrint()
        {
            CompetenceBasedAdaptionHandler.Instance.loggingPRA("******************************************************************************");
            CompetenceBasedAdaptionHandler.Instance.loggingPRA("       GAME SITUATION STRUCTURE:       ");
            CompetenceBasedAdaptionHandler.Instance.loggingPRA("initial  GS: ");
            CompetenceBasedAdaptionHandler.Instance.loggingPRA("             " + initialGameSituation.Id);
            CompetenceBasedAdaptionHandler.Instance.loggingPRA("contains GS: ");
            foreach (GameSituation gs in gameSituations)
                gs.diagnosticPrint();
            CompetenceBasedAdaptionHandler.Instance.loggingPRA("******************************************************************************");
        }

        #endregion Methods
    }
}
