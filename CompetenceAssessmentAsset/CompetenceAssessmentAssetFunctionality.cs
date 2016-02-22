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
using DomainModelAssetNameSpace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CompetenceAssessmentAssetNameSpace
{
    /// <summary>
    /// Singelton Class for handling Competence Assessment
    /// </summary>
    internal class CompetenceAssessmentHandler
    {
        #region AlgorithmParameters

        /// <summary>
        /// Algorithm variable for upgrading probabilities.
        /// </summary>
        internal double xi0 = 2;

        /// <summary>
        /// Algorithm variable for downgrading probabilities.
        /// </summary>
        internal double xi1 = 2;

        /// <summary>
        /// epsilon used for mantaining competence structure consistency 
        /// </summary>
        internal double epsilon = 0.000000001;

        /// <summary>
        /// Limit: Probabilities equal or higher as this value are assumed to indicate mastery of a competence by a learner 
        /// </summary>
        public double transitionProbability = 0.7;

        #endregion AlgorithmParameters

        //TODO: store/load competence state somewhere else
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
        /// Instance of the CompetenceAssessmentHandler - Singelton pattern
        /// </summary>
        private static CompetenceAssessmentHandler instance;

        /// <summary>
        /// Dictionary containing all key/value pairs of playerId and competence structure.
        /// </summary>
        private Dictionary<String, CompetenceStructure> competenceStructureDictionary = new Dictionary<String, CompetenceStructure>();

        /// <summary>
        /// Dictinary containing all competence states.
        /// </summary>
        private Dictionary<String, CompetenceState> competenceStates = new Dictionary<string, CompetenceState>();

        /// <summary>
        /// If true logging is done, otherwise no logging is done.
        /// </summary>
        private Boolean doLogging = true;

        #endregion Fields
        #region Constructors 

        /// <summary>
        /// Private ctor - Singelton pattern
        /// </summary>
        private CompetenceAssessmentHandler() { }

        #endregion Constructors
        #region Properties

        /// <summary>
        /// Getter for Instance of the CompetenceAssessmentHandler - Singelton pattern
        /// </summary>
        public static CompetenceAssessmentHandler Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new CompetenceAssessmentHandler();
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

        #endregion Properties
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
        /// Method for creating a competence-structure with an id (= playerId).
        /// </summary>
        /// 
        /// <param name="competenceStructureId"> String containing the competence-structure-id. Used to reference to this competence-structure. </param>
        /// <param name="dm"> Specifies Domainmodel used to create the competence-structure. </param>
        ///
        /// <returns>
        /// Competence-structure according to the specified domainmodel.
        /// </returns>
        internal CompetenceStructure createCompetenceStructure(String playerId, DomainModel dm)
        {
            if (competenceStructureDictionary.ContainsKey(playerId))
            {
                loggingCA("CompetenceStructure already exists with this id(" + playerId + ")!");
                return (null);
            }
            CompetenceStructure cst = new CompetenceStructure(dm);
            competenceStructureDictionary[playerId] = cst;
            return (cst);
        }

        /// <summary>
        /// Method for creating a competence state.
        /// </summary>
        /// 
        /// <param name="cst"> Specifies competence-structure this competence state is created for. </param>
        ///
        /// <returns>
        /// Competence state according to the specified competence-structure.
        /// </returns>
        internal CompetenceState createCompetenceState(String playerId, CompetenceStructure cst)
        {
            if (competenceStates.ContainsKey(playerId))
            {
                loggingCA("ERROR: Competence state to this player-Id already exists!");
                return null;
            }
            CompetenceState cs = new CompetenceState(cst);
            competenceStates[playerId] = cs;
            return cs;
        }

        #endregion InternalMethods
        #region PublicMethods

        /// <summary>
        /// Method for performing all neccessary operations to run update methods.
        /// </summary>
        /// 
        /// <param name="playerId"> Player Id which is created. </param>
        /// <param name="dm"> Specifies the domain model used for the following registration. </param>
        public void registerNewPlayer(String playerId, DomainModel dm)
        {
            CompetenceStructure cstr = createCompetenceStructure(playerId, dm);
            createCompetenceState(playerId, cstr);
        }

        /// <summary>
        /// Method for updating the competence state of a player.
        /// </summary>
        /// 
        /// <param name="playerId"> Player Id for the update - specifies for which player the competence state gets updated. </param>
        /// <param name="compList"> List of Strings - each String describes a competence.  </param>
        /// <param name="evidenceList"> Specifies if the evidences are speaking for or against the competence. </param>
        /// 
        public void updateCompetenceState(String playerId, List<String> compList, List<Boolean> evidenceList)
        {
            for (int i = 0; i < compList.Count; i++)
            {
                string evi = evidenceList[i] ? "up" : "down";
                loggingCA("updating " + compList[i] + ":" + evi);
            }

            if (!competenceStates.ContainsKey(playerId))
            {
                loggingCA("ERROR: There is no competence state with given playerId!");
                return;
            }

            if (!competenceStructureDictionary.ContainsKey(playerId))
            {
                loggingCA("ERROR: There is no competence structure for given playerId!");
                return;
            }

            CompetenceState csta = competenceStates[playerId];
            CompetenceStructure cstr = competenceStructureDictionary[playerId];

            cstr.updateCompetenceState(csta, compList, evidenceList);

        }

        /// <summary>
        /// Method for updating the competence state of a player.
        /// </summary>
        /// 
        /// <param name="playerId"> Player Id for the update - specifies for which player the competence state gets updated. </param>
        /// <param name="compList"> List of Strings - each String describes a competence.  </param>
        /// <param name="evidence"> Specifies if the evidences are speaking for or against the competence. </param>
        /// 
        public void updateCompetenceState(String playerId, List<String> compList, Boolean evidence)
        {
            List<Boolean> evidenceList = new List<Boolean>();
            foreach (String str in compList)
                evidenceList.Add(evidence);
            updateCompetenceState(playerId, compList, evidenceList);
        }

        /// <summary>
        /// Returns the competence state of a player.
        /// </summary>
        /// 
        /// <param name="playerId"> Player identification. </param>
        /// 
        /// <returns> Competence state of the specified player. </returns>
        public CompetenceState getCompetenceState(String playerId)
        {
            if (!competenceStates.ContainsKey(playerId))
            {
                loggingCA("Player id not associated with a competence state.");
                return null;
            }

            return competenceStates[playerId];
        }

        #endregion PublicMethods
        #region TestMethods

        /// <summary>
        /// Method for diagnostic logging.
        /// </summary>
        /// 
        /// <param name="msg"> Message to be logged. </param>
        internal void loggingCA(String msg, Severity severity = Severity.Information)
        {
            if (DoLogging)
            {
                if(competenceAssessmentAsset==null)
                    competenceAssessmentAsset = (CompetenceAssessmentAsset)AssetManager.Instance.findAssetByClass("CompetenceAssessmentAsset");
                competenceAssessmentAsset.Log(severity, "[CAA]: " + msg);
            }
        }

        /// <summary>
        /// Method calls all tests.
        /// </summary>
        public void performAllTests()
        {
            loggingCA("Competence assessment asset tests called: ");
            performTest1();
            performTest2();
            loggingCA("Competence assessment asset tests finished. ");
        }

        /// <summary>
        /// Creates example domainmodel through the domainmodelhandler and performes some updates.
        /// </summary>
        private void performTest1()
        {
            String testUser = "testUser";
            DomainModel dm = getDMA().getDomainModel(testUser);
            CompetenceStructure cst = createCompetenceStructure(testUser, dm);
            CompetenceState cs = createCompetenceState(testUser, cst);
            cs.print();

            //first update - upgrade
            List<String> compList = new List<string>();
            List<Boolean> evidenceList = new List<Boolean>();
            compList.Add("C1");
            evidenceList.Add(true);
            updateCompetenceState(testUser, compList, evidenceList);
            getCompetenceState(testUser).print();

            //second update - downgrade
            List<String> compList2 = new List<string>();
            List<Boolean> evidenceList2 = new List<Boolean>();
            compList2.Add("C1");
            evidenceList2.Add(false);
            updateCompetenceState(testUser, compList2, evidenceList2);
            getCompetenceState(testUser).print();
        }

        /// <summary>
        /// Test method printing out an example domain model. 
        /// </summary>
        private void performTest2()
        {
            DomainModel dm = getDMA().getDomainModel("dummyUser");
            dm.print();
        }

        #endregion TestMethods

    }

    /// <summary>
    /// Class representing the Competence-Tree of the Domainmodel.
    /// </summary>
    public class CompetenceStructure
    {
        #region Fields 

        /// <summary>
        /// Domainmodel-ID
        /// </summary>
        internal String domainModelId;

        /// <summary>
        /// List of competences forming the competence-structure
        /// </summary>
        internal List<Competence> competences = new List<Competence>();

        /// <summary>
        /// Algorithm-parameters for updating a competence state áccording to this competence-structure.
        /// </summary>
        private double xi0 = CompetenceAssessmentHandler.Instance.xi0;
        private double xi1 = CompetenceAssessmentHandler.Instance.xi1;
        private double epsilon = CompetenceAssessmentHandler.Instance.epsilon;

        #endregion Fields
        #region Constructors

        /// <summary>
        /// Constructor using a DomainModel.
        /// </summary>
        /// 
        /// <param name="dm"> DomainModel which is used to create the CompetenceStructure. </param>
        internal CompetenceStructure(DomainModel dm)
        {
            domainModelId = dm.metadata.id;

            //adding competences
            foreach (CompetenceDesc comd in dm.elements.competences.competenceList)
            {
                competences.Add(new Competence(comd.id, comd.title, this));
            }

            //adding prerequisites and successors
            foreach (CompetenceP comp in dm.relations.competenceprerequisites.competences)
            {
                getCompetenceById(comp.id).addPrerequisite(getCompetenceById(comp.prereqcompetence.id));
                getCompetenceById(comp.prereqcompetence.id).addSuccessor(getCompetenceById(comp.id));
            }


        }

        #endregion Constructors
        #region Methods

        /// <summary>
        /// Method for getting Competence by ID from competence structure.
        /// </summary>
        /// 
        /// <param name="id"> Unique competence-id within a competence structure. </param>
        ///
        /// <returns>
        /// Competence specified by the given id.
        /// </returns>
        internal Competence getCompetenceById(String id)
        {
            foreach (Competence com in competences)
            {
                if (com.id.Equals(id))
                {
                    return (com);
                }
            }
            return (null);
        }

        /// <summary>
        /// Diagnostic-method for displaying the competence structure
        /// </summary>
        public void print()
        {
            CompetenceAssessmentHandler.Instance.loggingCA("Printing competence-structure:");
            CompetenceAssessmentHandler.Instance.loggingCA("==============================");

            foreach (Competence com in competences)
            {
                com.print();
            }
        }

        /// <summary>
        /// Method for updating a competence state with a set of evidences.
        /// </summary>
        /// 
        /// <param name="cs"> Specifies competence state to update. </param>
        /// <param name="compList"> Speciefies for which Competences evidences are observed. </param>
        /// <param name="evidenceList"> Specifies if evidences are observed for (true) or against (false) possessing a competence. </param>
        internal void updateCompetenceState(CompetenceState cs, List<Competence> compList, List<Boolean> evidenceList)
        {
            Dictionary<string, double> sum = new Dictionary<string, double>();

            //initialise all sum-values with zero
            foreach (Competence comp in cs.getCurrentValues().Keys.ToList())
            {
                sum[comp.id] = 0.0;
            }

            Dictionary<string, double> tmp;
            for (int i = 0; i < compList.Count; i++)
            {
                tmp = updateCompetenceStateWithOneEvidence(cs, compList[i], evidenceList[i]);

                foreach (Competence comp in cs.getCurrentValues().Keys.ToList())
                {
                    sum[comp.id] = sum[comp.id] + tmp[comp.id];
                }
            }

            foreach (Competence comp in cs.getCurrentValues().Keys.ToList())
            {
                cs.setCompetenceValue(comp, sum[comp.id] / compList.Count);
            }

        }

        /// <summary>
        /// Method for updating a competence state with one evidences.
        /// </summary>
        /// 
        /// <param name="cs"> Specifies competence state to update. </param>
        /// <param name="comp"> Speciefies for which Competence evidence is observed. </param>
        /// <param name="evidence"> Specifies if evidence is observed for (true) or against (false) possessing a competence. </param>
        internal void updateCompetenceState(CompetenceState cs, String comp, Boolean evidence)
        {
            updateCompetenceState(cs, this.getCompetenceById(comp), evidence);
        }

        /// <summary>
        /// Method for updating a competence state with a set of evidences.
        /// </summary>
        /// 
        /// <param name="cs"> Specifies competence state to update. </param>
        /// <param name="compList"> Speciefies for which Competences (by id) evidences are observed. </param>
        /// <param name="evidenceList"> Specifies if evidences are observed for (true) or against (false) possessing a competence. </param>
        internal void updateCompetenceState(CompetenceState cs, List<String> compList, List<Boolean> evidenceList)
        {
            List<Competence> cList = new List<Competence>();
            foreach (String str in compList)
            {
                cList.Add(getCompetenceById(str));
            }

            updateCompetenceState(cs, cList, evidenceList);
        }

        /// <summary>
        /// Method for updating a competence state with one evidence.
        /// </summary>
        /// 
        /// <param name="cs"> Specifies competence state to update. </param>
        /// <param name="comp"> Speciefies for which Competence evidence is observed. </param>
        /// <param name="evidence"> Specifies if evidence is observed for (true) or against (false) possessing a competence. </param>
        internal void updateCompetenceState(CompetenceState cs, Competence comp, Boolean evidence)
        {
            List<Competence> compList = new List<Competence>();
            compList.Add(comp);
            List<Boolean> evidenceList = new List<Boolean>();
            evidenceList.Add(evidence);
            updateCompetenceState(cs, compList, evidenceList);
        }

        /// <summary>
        /// Method for updating a competence state with one evidence.
        /// </summary>
        /// 
        /// <param name="cs"> Specifies competence state to update. </param>
        /// <param name="com"> Specifies for which competence an evidence is available. </param>
        /// <param name="evidence"> Specifies if the evidence indicates possesion (true) of the competence or not (false). </param>
        ///
        /// <returns>
        /// Dictionary with key/value pairs of competence-id and updated probability of pessesing the competence. 
        /// </returns>
        internal Dictionary<string, double> updateCompetenceStateWithOneEvidence(CompetenceState cs, Competence com, Boolean evidence)
        {
            Dictionary<string, double> pairs = new Dictionary<string, double>();
            Double denominator;

            foreach (Competence comp in cs.getCurrentValues().Keys.ToList())
            {
                pairs[comp.id] = 0.0;
            }

            if (evidence)
                denominator = xi0 * cs.getValue(com.id) + (1 - cs.getValue(com.id));
            else
                denominator = cs.getValue(com.id) + xi1 * (1 - cs.getValue(com.id));

            foreach (Competence competence in this.competences)
            {
                if (com.isIndirectPrerequesiteOf(competence) && com.id != competence.id)
                {
                    if (evidence)
                        pairs[competence.id] = (xi0 * cs.getValue(competence.id)) / denominator;
                    else
                        pairs[competence.id] = cs.getValue(competence.id) / denominator;
                }
                else if (competence.isIndirectPrerequesiteOf(com))
                {
                    if (evidence)
                        pairs[competence.id] = (xi0 * cs.getValue(com.id) + (cs.getValue(competence.id) - cs.getValue(com.id))) / denominator;
                    else
                        pairs[competence.id] = (cs.getValue(com.id) + xi1 * (cs.getValue(competence.id) - cs.getValue(com.id))) / denominator;
                }
                else
                {
                    pairs[competence.id] = cs.getValue(competence.id);
                }
            }

            checkConsistency(pairs, evidence);

            return (pairs);
        }

        /// <summary>
        /// Method for checking (and restore) competence structure consistency - prerequisites must have a higher probability of beeing possessed.
        /// </summary>
        /// 
        /// <param name="pairs"> Dictionary with key/value pairs of competence-id and probability of pessessing the competence. </param>
        /// <param name="evidence"> Specifies if the evidence indicates possesion (true) of the competence or not (false). </param>
        private void checkConsistency(Dictionary<String, double> pairs, Boolean evidence)
        {
            Boolean changes = true;
            while (changes)
            {
                changes = false;
                foreach (Competence com1 in competences)
                {
                    foreach (Competence com2 in competences)
                    {
                        if (com1.id != com2.id && com1.isIndirectPrerequesiteOf(com2) && pairs[com1.id] <= pairs[com2.id])
                        {
                            if (evidence)
                                pairs[com1.id] = Math.Min(1 - epsilon, pairs[com2.id] + epsilon);
                            else
                                pairs[com2.id] = Math.Max(epsilon, pairs[com1.id] - epsilon);
                            changes = true;
                        }
                    }
                }
            }
        }

        #endregion Methods

    }

    /// <summary>
    /// Class representing a Competence in the Competence-Tree of the Domainmodel.
    /// </summary>
    public class Competence
    {
        #region Fields 

        /// <summary>
        /// Unique id within a competence-structure
        /// </summary>
        public string id;

        /// <summary>
        /// Human-readable name of the competence
        /// </summary>
        public string title;

        /// <summary>
        /// List of prerequisites to this competence
        /// </summary>
        public List<Competence> prerequisites = new List<Competence>();

        /// <summary>
        /// List of successors to this competence
        /// </summary>
        public List<Competence> successors = new List<Competence>();

        /// <summary>
        /// Competence-structure containing this competence
        /// </summary>
        public CompetenceStructure cst;

        #endregion Fields
        #region Constructors

        /// <summary>
        /// Competence ctor
        /// </summary>
        /// 
        /// <param name="newId"> Unique Competence Id within a competence structure. </param>
        /// <param name="newTitle"> Competence name/description. </param>
        /// <param name="newCst"> Spezifies CompetenceStructure this competence is contained in. </param>
        public Competence(String newId, String newTitle, CompetenceStructure newCst)
        {
            id = newId;
            title = newTitle;
            cst = newCst;
        }

        #endregion Constructors
        #region Methods

        /// <summary>
        /// Method for adding a prerequisite.
        /// </summary>
        /// 
        /// <param name="prerequisite"> Specifies prerequisite to be added to the competence. </param>
        public void addPrerequisite(Competence prerequisite)
        {
            prerequisites.Add(prerequisite);
        }

        /// <summary>
        /// Method for adding a successor.
        /// </summary>
        /// 
        /// <param name="successor"> Specifies successor to be added to the competence. </param>
        public void addSuccessor(Competence successor)
        {
            successors.Add(successor);
        }

        /// <summary>
        /// Diagnostic method for displaying a competence.
        /// </summary>
        public void print()
        {
            CompetenceAssessmentHandler.Instance.loggingCA("Competence: " + id);

            if (prerequisites.Count > 0)
            {
                CompetenceAssessmentHandler.Instance.loggingCA("Prerequisites: ");
            }
            foreach (Competence com in prerequisites)
            {
                CompetenceAssessmentHandler.Instance.loggingCA("       - " + com.id);
            }

            if (successors.Count > 0)
            {
                CompetenceAssessmentHandler.Instance.loggingCA("Successors: ");
            }
            foreach (Competence com in successors)
            {
                CompetenceAssessmentHandler.Instance.loggingCA("       - " + com.id);
            }
        }

        /// <summary>
        /// Method determining if the competence (this) is a (in)direct prerequisite of a given competence (com).
        /// </summary>
        /// 
        /// <param name="com"> Specifies for which competence the checking is done. </param>
        ///
        /// <returns>
        /// Boolean: True if the competence (this) is a (in)direct prerequisite of a given competence (com), false otherwise.
        /// </returns>
        public Boolean isIndirectPrerequesiteOf(Competence com)
        {
            if (this.id == com.id)
            {
                return (true);
            }
            else
            {
                if (com.prerequisites.Count == 0)
                    return (false);

                foreach (Competence c in com.prerequisites)
                {
                    if (this.isIndirectPrerequesiteOf(c))
                        return (true);
                }
                return (false);
            }
        }

        /// <summary>
        /// Method determining if the competence (this) is a (in)direct successor of a given competence (com).
        /// </summary>
        /// 
        /// <param name="com"> Specifies for which competence the checking is done. </param>
        ///
        /// <returns>
        /// Boolean: True if the competence (this) is a (in)direct successor of a given competence (com), false otherwise.
        /// </returns>
        public Boolean isIndirectSuccessorOf(Competence com)
        {
            return (!com.isIndirectPrerequesiteOf(this));
        }

        #endregion Methods

    }

    /// <summary>
    /// Class representing the competence-state of a learner.
    /// </summary>
    public class CompetenceState
    {
        #region Fields

        /// <summary>
        /// Dictionary containing the key/value pairs of competences and probability of possession assosiated with the competence
        /// </summary>
        Dictionary<Competence, double> pairs = new Dictionary<Competence, double>();

        /// <summary>
        /// Limit: Values of CompetenceAssessmentHandler.Instance.transitionProbability and above are assumed to indicate mastery of a competence by a learner 
        /// </summary>
        public double transitionProbability = CompetenceAssessmentHandler.Instance.transitionProbability;

        #endregion Fields
        #region Constructors

        /// <summary>
        /// CompetenceState ctor
        /// </summary>
        /// 
        /// <param name="cst"> Spezifies CompetenceStructure this CompetenceState is created for. </param>
        public CompetenceState(CompetenceStructure cst)
        {
            setInitialCompetenceState(cst);
        }

        #endregion Constructors
        #region Methods

        /// <summary>
        /// Method for accessing the current competence-state values.
        /// </summary>
        ///
        /// <returns>
        /// Dictionary containing Competences and the probability of possessing them.
        /// </returns>
        public Dictionary<Competence, double> getCurrentValues()
        {
            return (pairs);
        }

        /// <summary>
        /// Method for accessing the probability of possessing a competence.
        /// </summary>
        /// 
        /// <param name="competence"> Spezifies for which competence the probability is returned. </param>
        ///
        /// <returns>
        /// Probability of possessing the spezified competence.
        /// </returns>
        public double getValue(Competence competence)
        {
            return (pairs[competence]);
        }

        /// <summary>
        /// Method for accessing the probability of possessing a competence.
        /// </summary>
        /// 
        /// <param name="competence"> String- specifying for which competence the probability is returned. </param>
        ///
        /// <returns>
        /// Probability of possessing the specified competence - if available; -1 otherwise.
        /// </returns>
        public double getValue(String competenceId)
        {
            foreach (KeyValuePair<Competence, double> entry in pairs)
            {
                if (entry.Key.id == competenceId)
                    return (entry.Value);
            }
            return (-1.0);
        }


        /// <summary>
        /// Method for setting an initial competence state according to "Remarks on the Simplified Update Rule - Cord Hockemeyer"
        /// </summary>
        /// 
        /// <param name="cst"> Spezifies the competence structure for which the competence state is created. </param>
        private void setInitialCompetenceState(CompetenceStructure cst)
        {
            Dictionary<string, double> ups = new Dictionary<string, double>();
            Dictionary<string, double> downs = new Dictionary<string, double>();

            //initialize upsets/downsets
            foreach (Competence com in cst.competences)
            {
                ups[com.id] = 0.0;
                downs[com.id] = 0.0;
            }

            //calculate upsets/downsets
            foreach (Competence com1 in cst.competences)
            {
                foreach (Competence com2 in cst.competences)
                {
                    if (com1.isIndirectPrerequesiteOf(com2))
                    {
                        ups[com1.id] = ups[com1.id] + 1.0;
                        downs[com2.id] = downs[com2.id] + 1.0;
                    }
                }
            }

            foreach (Competence com in cst.competences)
            {
                pairs[com] = (cst.competences.Count + ups[com.id] - downs[com.id] + 1.0) / (2.0 * cst.competences.Count + 2.0);
            }

        }

        /// <summary>
        /// Diagnostic method for displaying competence state
        /// </summary>
        public void print()
        {
            CompetenceAssessmentHandler.Instance.loggingCA("Competence State:");
            //CompetenceAssessmentHandler.Instance.loggingCA("=================");
            String str = "";
            foreach (var pair in pairs)
            {
                str += "(" + pair.Key.id + ":" + Math.Round(pair.Value, 2) + ")";
                //CompetenceAssessmentHandler.Instance.loggingCA("Key: " + pair.Key.id + " Value: " + Math.Round(pair.Value,2));
            }
            CompetenceAssessmentHandler.Instance.loggingCA(str);

        }

        /// <summary>
        /// Method for setting a competence probability by competence name
        /// </summary>
        /// 
        /// <param name="str"> String- specifying for which competence the probability is set. </param>
        /// <param name="v"> Probability value </param>
        private void setCompetenceValue(string str, double v)
        {
            foreach (KeyValuePair<Competence, double> entry in pairs)
            {
                if (entry.Key.id == str)
                    pairs[entry.Key] = v; ;
            }

        }

        /// <summary>
        /// Method for setting a competence probability by competence 
        /// </summary>
        /// 
        /// <param name="str"> Specifying for which competence the probability is set. </param>
        /// <param name="v"> Probability value </param>
        public void setCompetenceValue(Competence com, double v)
        {
            pairs[com] = v; ;
        }

        /// <summary>
        /// Returns all mastered Competences.
        /// </summary>
        /// 
        /// <returns> List of all Competences which are assumed to be mastered. </returns>
        public List<Competence> getMasteredCompetences()
        {
            List<Competence> mastered = new List<Competence>();

            foreach (KeyValuePair<Competence, double> entry in pairs)
            {
                if (entry.Value >= transitionProbability)
                    mastered.Add(entry.Key);
            }

            return mastered;
        }

        /// <summary>
        /// Returns all mastered Competences as string-list.
        /// </summary>
        /// 
        /// <returns> List of all Competences as strings which are assumed to be mastered. </returns>
        public List<String> getMasteredCompetencesString()
        {
            List<Competence> mastered = getMasteredCompetences();
            List<String> masteredString = new List<string>();
            foreach (Competence com in mastered)
                masteredString.Add(com.id);

            return masteredString;
        }

        #endregion Methods

    }
}
