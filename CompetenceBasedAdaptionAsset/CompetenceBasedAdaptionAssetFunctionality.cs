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
        private Dictionary<String, GameSituation> currentGameSituations = new Dictionary<string, GameSituation>();

        //TODO: List of players -> game situation structure?
        //storage gss?
        /// <summary>
        /// Dictionary storing all game situation structures with player id as key.
        /// </summary>
        private Dictionary<String, GameSituationStructure> gameSituationStructures = new Dictionary<string, GameSituationStructure>();

        /// <summary>
        /// Storage of player id and game situation counter - how often has player played the game situations
        /// </summary>
        private Dictionary<String, Dictionary<GameSituation, int>> gameSituationHistory = new Dictionary<string, Dictionary<GameSituation, int>>();

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

        //TODO: GameSituation storage somewhere else?!
        /// <summary>
        /// Method returning the current game situation of an player by playerId.
        /// </summary>
        /// 
        /// <param name="playerId"> Identification of a player. </param>
        /// 
        /// <returns> GameSituation the player is currently in. </returns>
        internal GameSituation getCurrentGameSituation(String playerId)
        {
            if (!currentGameSituations.ContainsKey(playerId))
            {
                loggingPRA("Requested playerId is not associated with a current GameSituation!");
                return null;
            }

            return currentGameSituations[playerId];
        }

        /// <summary>
        /// Stores a game situation to a given player id as current game situation.
        /// </summary>
        /// 
        /// <param name="playerId"> Player Identification. </param>
        /// <param name="gs"> Game situation which is set to be the current GS for the specified player. </param>
        internal void setCurrentGameSituation(String playerId, GameSituation gs)
        {
            currentGameSituations[playerId] = gs;
        }

        /// <summary>
        /// Method returning the game situation structure of a player associated with the id.
        /// </summary>
        /// 
        /// <param name="playerId"> Player identification. </param>
        /// 
        /// <returns>Game situation structure for the supplied player id. </returns>
        internal GameSituationStructure getGameSituationStructure(String playerId)
        {
            if (!gameSituationStructures.ContainsKey(playerId))
            {
                loggingPRA("Requested playerId is not associated with a GameSituationStructure!");
                return null;
            }

            return gameSituationStructures[playerId];
        }

        /// <summary>
        /// Sets a game situation structure to a player.
        /// </summary>
        /// 
        /// <param name="playerId"> Player identification. </param>
        /// <param name="gss"> Game situation structure which gets linked to the player id. </param>
        internal void setGameSituationStructure(String playerId, GameSituationStructure gss)
        {
            gameSituationStructures[playerId] = gss;
        }

        /// <summary>
        /// Returns the game situation history as a player.
        /// </summary>
        /// 
        /// <param name="playerId"> Player Identification. </param>
        /// 
        /// <returns> A dictionary containing the game situations as keys and the number of times they where player by the player as values. </returns>
        internal Dictionary<GameSituation, int> getGameSituationHistory(String playerId)
        {
            if (!gameSituationHistory.ContainsKey(playerId))
            {
                loggingPRA("No game situation history available for this player - creating a new one.");
                GameSituationStructure gss = getGameSituationStructure(playerId);
                Dictionary<GameSituation, int> dgh = new Dictionary<GameSituation, int>();
                foreach (GameSituation gs in gss.GameSituations)
                {
                    dgh[gs] = 0;
                }
                gameSituationHistory[playerId] = dgh;
                return dgh;
            }
            else
                return gameSituationHistory[playerId];
        }

        /// <summary>
        /// Increments the integer counting the number of times a player has player a game situation.
        /// </summary>
        /// 
        /// <param name="playerId"> Player Identification. </param>
        /// 
        /// <param name="gs"> Game situation played. </param>
        internal void updateGameSituationHistory(String playerId, GameSituation gs)
        {
            gameSituationHistory[playerId][gs]++;
        }

        #endregion InternalMethods
        #region PublicMethods

        /// <summary>
        /// Returns the Id of the next game situation.
        /// </summary>
        /// 
        /// <param name="playerId"> Player Identification. </param>
        /// 
        /// <returns> Identification of the next game situation proposed for the player. </returns>
        public String getNextGameSituationId(String playerId)
        {
            if (!gameSituationStructures.ContainsKey(playerId))
            {
                loggingPRA("The game situation structure for the specified player does not exist.");
                return null;
            }
            GameSituationStructure gss = gameSituationStructures[playerId];
            GameSituation nextGS = gss.determineNextGameSituation(playerId);
            if (nextGS != null)
            {
                updateGameSituationHistory(playerId, nextGS);
                return nextGS.Id;
            }
            return null;
        }

        /// <summary>
        /// Method returning the id of the current game situation player by the player.
        /// </summary>
        /// 
        /// <param name="playerId"> Player Identification. </param>
        /// 
        /// <returns> String containing the game situation identification. </returns>
        public String getCurrentGameSituationId(String playerId)
        {
            GameSituation gs = getCurrentGameSituation(playerId);
            if (gs == null)
                return null;
            return getCurrentGameSituation(playerId).Id;
        }

        /// <summary>
        /// Method updating the competence state due to a game situation success/failure.
        /// </summary>
        /// 
        /// <param name="success"> If this value is set to true the player has successfully completed the game situation, otherwise not. </param>
        /// <param name="playerId"> Id of the player in the game situation. </param>
        public void setGameSituationUpdate(String playerId, Boolean success)
        {
            loggingPRA("Gamesituation completed - sending evidence to update competences.");
            GameSituation gs = getCurrentGameSituation(playerId);
            List<Boolean> successList = new List<bool>();
            foreach (String com in gs.Competences)
                successList.Add(success);
            getCAA().updateCompetenceState(playerId, gs.Competences, successList);
        }

        /// <summary>
        /// Method for performing all neccessary operations to run update methods.
        /// </summary>
        /// 
        /// <param name="playerId"> Player Id which is created. </param>
        /// <param name="dm"> Specifies the domain model used for the following registration. </param>
        public void registerNewPlayer(String playerId, DomainModel dm)
        {
            GameSituationStructure gss = new GameSituationStructure(dm);
            setGameSituationStructure(playerId, gss);
            setCurrentGameSituation(playerId, gss.InitialGameSituation);
            getCAA().registerNewPlayer(playerId, dm);
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
                    competenceBasedAdaptionAsset = (CompetenceBasedAdaptionAsset)AssetManager.Instance.findAssetByClass("CompetenceRecommendationAsset");
                competenceBasedAdaptionAsset.Log(severity, "[CRA]: " + msg);
            }
        }

        /// <summary>
        /// Method calls all tests.
        /// </summary>
        public void performAllTests()
        {
            loggingPRA("Competence recommendation asset tests called: ");
            //performTest1();
            performTest2();
            loggingPRA("Competence recommendation asset tests finished. ");
        }

        /// <summary>
        /// Creates a gamestructure and competence state/structure for performing some updates.
        /// </summary>
        private void performTest1()
        {
            GameSituationStructure gss = createExampleGSS();
            gss.diagnosticPrint();
        }

        /// <summary>
        /// Test method - perform updates according to console input
        /// </summary>
        private void performTest2()
        {
            String player = "p1";
            registerNewPlayer(player, getDMA().getDomainModel(player));
            GameSituationStructure gss = getGameSituationStructure(player);
            String gs = getCurrentGameSituationId(player);
            Dictionary<String,double> cs = getCAA().getCompetenceState(player);
            String str = "";
            foreach(KeyValuePair<String,double> pair in cs)
                str += "(" + pair.Key + ":" + Math.Round(pair.Value, 2) + ")";
            loggingPRA(str);

            string line = "";
            while (gs != null)
            {
                while (!line.Equals("s") && !line.Equals("f") && !line.Equals("e"))
                {
                    loggingPRA("Entering game situation " + gs + ". How did the player performe (s-success,f-failure,e-exit)?");
                    line = Console.ReadLine();
                }
                if (!line.Equals("e"))
                {
                    loggingPRA("current gs:" + getCurrentGameSituationId(player));
                    if (line.Equals("s"))
                        setGameSituationUpdate(player, true);
                    else if (line.Equals("f"))
                        setGameSituationUpdate(player, false);
                    cs = getCAA().getCompetenceState(player);
                    str = "";
                    foreach (KeyValuePair<String, double> pair in cs)
                        str += "(" + pair.Key + ":" + Math.Round(pair.Value, 2) + ")";
                    loggingPRA(str);
                }
                else
                {
                    loggingPRA("Test Ended.");
                    return;
                }
                line = "";
                gs = getNextGameSituationId(player);
            }
            loggingPRA("Games end reached!");
        }

        /// <summary>
        /// Creates an example game situation structure for testing.
        /// </summary>
        /// 
        /// <returns> An example game situation sturucture. </returns>
        private GameSituationStructure createExampleGSS()
        {
            GameSituationStructure gss = new GameSituationStructure();
            GameSituation gs1 = new GameSituation("gs1");
            GameSituation gs2 = new GameSituation("gs2");
            GameSituation gs3 = new GameSituation("gs3");
            GameSituation gs4 = new GameSituation("gs4");
            GameSituation gs5 = new GameSituation("gs5");
            GameSituation gs6 = new GameSituation("gs6");
            GameSituation gs7 = new GameSituation("gs7");
            GameSituation gs8 = new GameSituation("gs8");
            GameSituation gs9 = new GameSituation("gs9");
            GameSituation gs10 = new GameSituation("gs10");

            //define competences included in gs1
            String[] c1Array = { "C1" };
            List<String> c1 = new List<String>(c1Array);
            //define gs which are possible successors of gs1
            GameSituation[] gs1Array = { gs1, gs2, gs3, gs4, gs5, gs6, gs7, gs8, gs9, gs10 };
            List<GameSituation> gs1L = new List<GameSituation>(gs1Array);
            gs1.Id = "gs1";
            gs1.Successors = gs1L;
            gs1.Competences = c1;

            //define competences included in gs2
            String[] c2Array = { "C2" };
            List<String> c2 = new List<String>(c2Array);
            //define gs which are possible successors of gs2
            GameSituation[] gs2Array = { gs1, gs2, gs3, gs4, gs5, gs6, gs7, gs8, gs9, gs10 };
            List<GameSituation> gs2L = new List<GameSituation>(gs2Array);
            gs2.Id = "gs2";
            gs2.Successors = gs2L;
            gs2.Competences = c2;

            //define competences included in gs3
            String[] c3Array = { "C3" };
            List<String> c3 = new List<String>(c3Array);
            //define gs which are possible successors of gs3
            GameSituation[] gs3Array = { gs1, gs2, gs3, gs4, gs5, gs6, gs7, gs8, gs9, gs10 };
            List<GameSituation> gs3L = new List<GameSituation>(gs3Array);
            gs3.Id = "gs3";
            gs3.Successors = gs3L;
            gs3.Competences = c3;

            //define competences included in gs4
            String[] c4Array = { "C4" };
            List<String> c4 = new List<String>(c4Array);
            //define gs which are possible successors of gs4
            GameSituation[] gs4Array = { gs1, gs2, gs3, gs4, gs5, gs6, gs7, gs8, gs9, gs10 };
            List<GameSituation> gs4L = new List<GameSituation>(gs4Array);
            gs4.Id = "gs4";
            gs4.Successors = gs4L;
            gs4.Competences = c4;

            //define competences included in gs5
            String[] c5Array = { "C1", "C2", "C5" };
            List<String> c5 = new List<String>(c5Array);
            //define gs which are possible successors of gs5
            GameSituation[] gs5Array = { gs1, gs2, gs3, gs4, gs5, gs6, gs7, gs8, gs9, gs10 };
            List<GameSituation> gs5L = new List<GameSituation>(gs5Array);
            gs5.Id = "gs5";
            gs5.Successors = gs5L;
            gs5.Competences = c5;

            //define competences included in gs6
            String[] c6Array = { "C4", "C6" };
            List<String> c6 = new List<String>(c6Array);
            //define gs which are possible successors of gs6
            GameSituation[] gs6Array = { gs1, gs2, gs3, gs4, gs5, gs6, gs7, gs8, gs9, gs10 };
            List<GameSituation> gs6L = new List<GameSituation>(gs6Array);
            gs6.Id = "gs6";
            gs6.Successors = gs6L;
            gs6.Competences = c6;

            //define competences included in gs7
            String[] c7Array = { "C4", "C7" };
            List<String> c7 = new List<String>(c7Array);
            //define gs which are possible successors of gs7
            GameSituation[] gs7Array = { gs1, gs2, gs3, gs4, gs5, gs6, gs7, gs8, gs9, gs10 };
            List<GameSituation> gs7L = new List<GameSituation>(gs7Array);
            gs7.Id = "gs7";
            gs7.Successors = gs7L;
            gs7.Competences = c7;

            //define competences included in gs8
            String[] c8Array = { "C3", "C6", "C8" };
            List<String> c8 = new List<String>(c8Array);
            //define gs which are possible successors of gs8
            GameSituation[] gs8Array = { gs1, gs2, gs3, gs4, gs5, gs6, gs7, gs8, gs9, gs10 };
            List<GameSituation> gs8L = new List<GameSituation>(gs8Array);
            gs8.Id = "gs8";
            gs8.Successors = gs8L;
            gs8.Competences = c8;

            //define competences included in gs9
            String[] c9Array = { "C5", "C8", "C9" };
            List<String> c9 = new List<String>(c9Array);
            //define gs which are possible successors of gs9
            GameSituation[] gs9Array = { gs1, gs2, gs3, gs4, gs5, gs6, gs7, gs8, gs9, gs10 };
            List<GameSituation> gs9L = new List<GameSituation>(gs9Array);
            gs9.Id = "gs9";
            gs9.Successors = gs9L;
            gs9.Competences = c9;

            //define competences included in gs10
            String[] c10Array = { "C7", "C9", "C10" };
            List<String> c10 = new List<String>(c10Array);
            //define gs which are possible successors of gs1
            GameSituation[] gs10Array = { gs1, gs2, gs3, gs4, gs5, gs6, gs7, gs8, gs9, gs10 };
            List<GameSituation> gs10L = new List<GameSituation>(gs10Array);
            gs10.Id = "gs10";
            gs10.Successors = gs10L;
            gs10.Competences = c10;

            GameSituation[] gsArray = { gs1, gs2, gs3, gs4, gs5, gs6, gs7, gs8, gs9, gs10 };
            List<GameSituation> gsList = new List<GameSituation>(gsArray);
            gss.GameSituations = gsList;
            gss.InitialGameSituation = gs1;

            return gss;
        }

        #endregion TestMethod
    }

    /// <summary>
    /// Class describing a game situation.
    /// </summary>
    internal class GameSituation
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
        internal List<String> Competences
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
        internal String Id
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
        internal List<GameSituation> Successors
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
    internal class GameSituationStructure
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
            foreach (Learningobject lo in dm.elements.learningobjects.learningobjectList)
                gameSituations.Add(new GameSituation(lo.id));
            //adding competences to the gs in the structure
            foreach (LearningobjectRelation lor in dm.relations.learningobjects.learningobjects)
                getGameSituationById(lor.id).Competences.Add(lor.competence.id);
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
        internal List<GameSituation> GameSituations
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
        internal GameSituation InitialGameSituation
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
        /// <param name="playerId"> Player Identification. </param>
        /// 
        /// <returns> The next game situation for the specified player. </returns>
        internal GameSituation determineNextGameSituation(String playerId)
        {
            CompetenceBasedAdaptionHandler.Instance.loggingPRA("determining next game situation for player " + playerId);
            GameSituation currentGS = CompetenceBasedAdaptionHandler.Instance.getCurrentGameSituation(playerId);
            ///tmp line
            //OLD:
            //CompetenceState cs = CompetenceAssessmentHandler.Instance.getCompetenceState(playerId);
            //List<String> mastered = cs.getMasteredCompetencesString();
            //NEW:
            Dictionary<String, double> cs = CompetenceBasedAdaptionHandler.Instance.getCAA().getCompetenceState(playerId);
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
            Dictionary<GameSituation, int> gameSituationHistory = CompetenceBasedAdaptionHandler.Instance.getGameSituationHistory(playerId);
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

            CompetenceBasedAdaptionHandler.Instance.setCurrentGameSituation(playerId, minPlayedGS);

            return minPlayedGS;
        }

        /// <summary>
        /// Prints out the GSS.
        /// </summary>
        internal void diagnosticPrint()
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
