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
  
  Development done by Cognitive Science Section (CSS) 
  at Knowlge Technologies Institute (KTI)at Graz University of Technology (TUGraz).
  http://kti.tugraz.at/css/

  Created by: Matthias Maurer, TUGraz <mmaurer@tugraz.at>
  Changed by: Matthias Maurer, TUGraz <mmaurer@tugraz.at>
  Changed on: 2016-02-10
*/


using AssetManagerPackage;
using AssetPackage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace DomainModelAssetNameSpace
{

    /// <summary>
    /// Singelton Class for handling (read DM from Web/File, store DM to File) the Domainmodel (DM)
    /// </summary>
    internal class DomainModelHandler
    {
        #region Fields

        /// <summary>
        /// Instance of the DomainModelAsset
        /// </summary>
        private DomainModelAsset domainModelAsset = null;

        /// <summary>
        /// Instance of the class DomainModelHandler - Singelton pattern
        /// </summary>
        private static DomainModelHandler instance;

        /// <summary>
        /// If true logging is done, otherwise no logging is done.
        /// </summary>
        private Boolean doLogging = true;

        /// <summary>
        /// Run-time Asset storage of domain models.
        /// </summary>
        private Dictionary<String, DomainModel> domainModels = new Dictionary<string, DomainModel>();

        #endregion Fields
        #region Constructors

        /// <summary>
        /// private DomainModelHandler-ctor for Singelton-pattern 
        /// </summary>
        private DomainModelHandler() { }

        #endregion Constructors
        #region Properties

        /// <summary>
        /// Getter for Instance of the DomainModelHandler - Singelton pattern
        /// </summary>
        public static DomainModelHandler Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new DomainModelHandler();
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
        #region PublicMethods 

        /// <summary>
        /// Method returning domain model either from the run-tima asset storage if available or from specified (default) source(File/Web).
        /// </summary>
        /// 
        /// <param name="playerId"> Id of the player for which the domain model is requested. </param>
        /// 
        /// <returns> The domein model associated with the player-id. </returns>
        internal DomainModel getDomainModel(String playerId)
        {
            if (domainModels.ContainsKey(playerId))
                return domainModels[playerId];
            DomainModel dm = loadDefaultDomainModel("dm.xml");
            domainModels[playerId] = dm;
            return dm;
        }

        #endregion PublicMethods 
        //TODO: default domain model loading behaviour
        #region InternalMethods

        /// <summary>
        /// Method loading domain model - location specified by settings.
        /// </summary>
        /// <returns></returns>
        internal DomainModel loadDefaultDomainModel(string fileId)
        {
            IDataStorage ids = (IDataStorage) AssetManager.Instance.Bridge;
            if (ids != null)
            {
                loggingDM("Loading DomainModel from File.");
                return (this.getDMFromXmlString(ids.Load(fileId)));
            }
            else
            {
                loggingDM("Loading example DomainModel.");
                return createExampleDomainModel();
            }
        }

        /// <summary>
        /// Method for deserialization of a XML-String to the coressponding Domainmodel.
        /// </summary>
        /// 
        /// <param name="str"> String containing the XML-Domainmodel for deserialization. </param>
        ///
        /// <returns>
        /// DomainModel-type coressponding to the parameter "str" after deserialization.
        /// </returns>
        internal DomainModel getDMFromXmlString(String str)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(DomainModel));
            using (TextReader reader = new StringReader(str))
            {
                DomainModel result = (DomainModel)serializer.Deserialize(reader);
                return (result);
            }
        }

        /*
        /// <summary>
        /// Method for requesting a XML-Domainmodel from a website and returning the coressponding DomainModel.
        /// </summary>
        /// 
        /// <param name="url"> Website URL containing the DomainModel. </param>
        ///
        /// <returns>
        /// DomainModel-type coressponding to the XML-Domainmodel on the spezified website.
        /// </returns>
        internal DomainModel getDMFromWeb(String url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream resStream = response.GetResponseStream();

            StreamReader reader = new StreamReader(resStream);
            string dm = reader.ReadToEnd();

            return (getDMFromXmlString(dm));
        }
        */

        /// <summary>
        /// Method for storing a DomainModel as XML in a File.
        /// </summary>
        /// 
        /// <param name="dm"> Domainmodel to store. </param>
        /// <param name="fileId"> String containing the file identification. </param>
        internal void writeDMToFile(DomainModel dm, String fileId)
        {
            IDataStorage ids = (IDataStorage)AssetManager.Instance.Bridge;
            if (ids != null)
            {
                loggingDM("Storing DomainModel to File.");
                ids.Save(fileId, dm.toXmlString());
            }
            else
                loggingDM("No IDataStorage - Bridge implemented!",Severity.Warning);

        }
        

        #endregion InternalMethods
        #region TestMethods

        /// <summary>
        /// Diagnostic logging method.
        /// </summary>
        /// 
        /// <param name="msg"> String to be logged.  </param>
        /// <param name="severity"> Severity of the logging-message, optional. </param>
        internal void loggingDM(String msg, Severity severity = Severity.Information)
        {
            if (DoLogging)
            {
                if(domainModelAsset==null)
                    domainModelAsset = (DomainModelAsset) AssetManager.Instance.findAssetByClass("DomainModelAsset");
                domainModelAsset.Log(severity, "[DMA]: " +msg);
            }
        }

        /// <summary>
        /// Method calls all tests.
        /// </summary>
        public void performAllTests()
        {
            loggingDM("Domain model asset tests called: ");
            performTest1();
            loggingDM("Domain model asset tests finished. ");
        }

        /// <summary>
        /// Creates example DomainModel; stores, loads and outputs this DomainModel from File.
        /// </summary>
        private void performTest1()
        {
            DomainModel dm = createExampleDomainModel();
            string fileId = "DomainModelTestId.xml";
            writeDMToFile(dm, fileId);
            DomainModel dm2 = loadDefaultDomainModel(fileId);
            dm2.print();
        }

        /// <summary>
        /// Method creating an example domain model
        /// </summary>
        /// <returns></returns>
        public DomainModel createExampleDomainModel()
        {
            DomainModel dm = new DomainModel();

            Metadata metadata = new Metadata();
            metadata.id = "exampleId";
            metadata.title = "exampleTitle";

            //Competences
            Elements elements = new Elements();
            CompetenceList cl = new CompetenceList();
            CompetenceDesc cd1 = new CompetenceDesc("C1");
            CompetenceDesc cd2 = new CompetenceDesc("C2");
            CompetenceDesc cd3 = new CompetenceDesc("C3");
            CompetenceDesc cd4 = new CompetenceDesc("C4");
            CompetenceDesc cd5 = new CompetenceDesc("C5");
            CompetenceDesc cd6 = new CompetenceDesc("C6");
            CompetenceDesc cd7 = new CompetenceDesc("C7");
            CompetenceDesc cd8 = new CompetenceDesc("C8");
            CompetenceDesc cd9 = new CompetenceDesc("C9");
            CompetenceDesc cd10 = new CompetenceDesc("C10");
            CompetenceDesc[] cdArray = { cd1, cd2, cd3, cd4, cd5, cd6, cd7, cd8, cd9, cd10 };
            List<CompetenceDesc> cdList = new List<CompetenceDesc>(cdArray);
            cl.competenceList = cdList;
            elements.competences = cl;

            //Game situations
            LearningobjectsList lol = new LearningobjectsList();
            Learningobject lo1 = new Learningobject("gs1");
            Learningobject lo2 = new Learningobject("gs2");
            Learningobject lo3 = new Learningobject("gs3");
            Learningobject lo4 = new Learningobject("gs4");
            Learningobject lo5 = new Learningobject("gs5");
            Learningobject lo6 = new Learningobject("gs6");
            Learningobject lo7 = new Learningobject("gs7");
            Learningobject lo8 = new Learningobject("gs8");
            Learningobject lo9 = new Learningobject("gs9");
            Learningobject lo10 = new Learningobject("gs10");
            Learningobject[] loArray = { lo1, lo2, lo3, lo4, lo5, lo6, lo7, lo8, lo9, lo10 };
            List<Learningobject> loList = new List<Learningobject>(loArray);
            lol.learningobjectList = loList;
            elements.learningobjects = lol;

            //Competences prerequisites
            Relations relations = new Relations();
            CompetenceprerequisitesList cpl = new CompetenceprerequisitesList();
            CompetenceP cp1 = new CompetenceP("C5", "C1");
            CompetenceP cp2 = new CompetenceP("C5", "C2");
            CompetenceP cp3 = new CompetenceP("C6", "C4");
            CompetenceP cp4 = new CompetenceP("C7", "C4");
            CompetenceP cp5 = new CompetenceP("C8", "C3");
            CompetenceP cp6 = new CompetenceP("C8", "C6");
            CompetenceP cp7 = new CompetenceP("C9", "C5");
            CompetenceP cp10 = new CompetenceP("C9", "C8");
            CompetenceP cp8 = new CompetenceP("C10", "C9");
            CompetenceP cp9 = new CompetenceP("C10", "C7");
            CompetenceP[] cpArray = { cp1, cp2, cp3, cp4, cp5, cp6, cp7, cp8, cp9, cp10 };
            List<CompetenceP> cpList = new List<CompetenceP>(cpArray);
            cpl.competences = cpList;
            relations.competenceprerequisites = cpl;

            //assignmend of competences to game situations (=learning objects)
            LearningobjectsRelationList lorl = new LearningobjectsRelationList();
            LearningobjectRelation lor1 = new LearningobjectRelation("gs1", "C1");
            LearningobjectRelation lor2 = new LearningobjectRelation("gs2", "C2");
            LearningobjectRelation lor3 = new LearningobjectRelation("gs3", "C3");
            LearningobjectRelation lor4 = new LearningobjectRelation("gs4", "C4");
            LearningobjectRelation lor5 = new LearningobjectRelation("gs5", "C5");
            LearningobjectRelation lor6 = new LearningobjectRelation("gs5", "C1");
            LearningobjectRelation lor7 = new LearningobjectRelation("gs5", "C2");
            LearningobjectRelation lor8 = new LearningobjectRelation("gs6", "C6");
            LearningobjectRelation lor9 = new LearningobjectRelation("gs6", "C4");
            LearningobjectRelation lor10 = new LearningobjectRelation("gs7", "C4");
            LearningobjectRelation lor11 = new LearningobjectRelation("gs7", "C7");
            LearningobjectRelation lor12 = new LearningobjectRelation("gs8", "C8");
            LearningobjectRelation lor13 = new LearningobjectRelation("gs8", "C6");
            LearningobjectRelation lor14 = new LearningobjectRelation("gs8", "C3");
            LearningobjectRelation lor15 = new LearningobjectRelation("gs9", "C9");
            LearningobjectRelation lor16 = new LearningobjectRelation("gs9", "C5");
            LearningobjectRelation lor17 = new LearningobjectRelation("gs9", "C8");
            LearningobjectRelation lor18 = new LearningobjectRelation("gs10", "C10");
            LearningobjectRelation lor19 = new LearningobjectRelation("gs10", "C9");
            LearningobjectRelation lor20 = new LearningobjectRelation("gs10", "C7");
            LearningobjectRelation[] lorArray = { lor1, lor2, lor3, lor4, lor5, lor6, lor7, lor8, lor9, lor10, lor11, lor12, lor13, lor14, lor15, lor16, lor17, lor18, lor19, lor20 };
            List<LearningobjectRelation> lorList = new List<LearningobjectRelation>(lorArray);
            lorl.learningobjects = lorList;
            relations.learningobjects = lorl;

            dm.version = "1.0";
            dm.elements = elements;
            dm.metadata = metadata;
            dm.relations = relations;

            return dm;
        }

        #endregion TestMethods

    }

    /// <summary>
    /// Classes for Serializing/Deserializing Domainmodels (e.g. DM like http://css-kmi.tugraz.at:8080/compod/rest/getdomainmodel?id=isr2013)
    /// </summary>
    #region Serializing 

    /// <summary>
    /// Class containing all Domainmodel data.
    /// </summary>
    [XmlRoot("domainmodel")]
    public class DomainModel
    {
        #region Properties

        [XmlElement("metadata")]
        public Metadata metadata { get; set; }

        [XmlElement("elements")]
        public Elements elements { get; set; }

        [XmlElement("relations")]
        public Relations relations { get; set; }

        [XmlAttribute("version")]
        public String version { get; set; }

        #endregion Properties

        /// <summary>
        /// Diagnostic-method printing Domainmodel data
        /// </summary>
        public void print()
        {
            DomainModelHandler.Instance.loggingDM("Printing out DM:");
            DomainModelHandler.Instance.loggingDM("-Version: " + version);
            metadata.print();
            elements.print();
            relations.print();
        }

        #region Methods

        /*
        public void writeToFile(String pathToFile)
        {
            DomainModelHandler.Instance.writeDMToFile(this, pathToFile);
        }
        */

        public String toXmlString()
        {
            try
            {
                var xmlserializer = new XmlSerializer(typeof(DomainModel));
                var stringWriter = new StringWriter();
                using (var writer = XmlWriter.Create(stringWriter))
                {
                    xmlserializer.Serialize(writer, this);
                    String xml = stringWriter.ToString();

                    return (xml);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred", ex);
            }
        }

        #endregion Methods
    }

    /// <summary>
    /// Class containing Methadata of the Domainmodel.
    /// </summary>
    public class Metadata
    {
        #region Properties

        [XmlElement("id")]
        public string id { get; set; }
        [XmlElement("title")]
        public string title { get; set; }
        [XmlElement("description")]
        public string description { get; set; }
        [XmlElement("owner")]
        public string owner { get; set; }
        [XmlElement("permission")]
        public string permission { get; set; }
        [XmlElement("created")]
        public string created { get; set; }
        [XmlElement("modified")]
        public string modified { get; set; }

        #endregion Properties

        /// <summary>
        /// Diagnostic-method printing Domainmodel data
        /// </summary>
        public void print()
        {
            DomainModelHandler.Instance.loggingDM("-Printing out metadata:");
            DomainModelHandler.Instance.loggingDM("--id:" + id);
            DomainModelHandler.Instance.loggingDM("--title:" + title);
            DomainModelHandler.Instance.loggingDM("--describtion:" + description);
            DomainModelHandler.Instance.loggingDM("--owner:" + owner);
            DomainModelHandler.Instance.loggingDM("--permission:" + permission);
            DomainModelHandler.Instance.loggingDM("--created:" + created);
            DomainModelHandler.Instance.loggingDM("--modified:" + modified);
        }
    }

    public class Elements
    {
        #region Properties

        //TODO Liste actionverbs
        //TODO Liste concepts
        [XmlElement("competences")]
        public CompetenceList competences { get; set; }
        [XmlElement("learningobjects")]
        public LearningobjectsList learningobjects { get; set; }
        [XmlElement("problems")]
        public ProblemsList problems { get; set; }

        #endregion Properties

        /// <summary>
        /// Diagnostic-method printing Domainmodel data
        /// </summary>
        public void print()
        {
            DomainModelHandler.Instance.loggingDM("-Printing out elements:");
            if (competences != null)
                competences.print();
            if (learningobjects != null)
                learningobjects.print();
            if (problems != null)
                problems.print();
        }
    }

    public class ProblemsList
    {
        #region Properties

        [XmlElement("problem")]
        public List<Problem> problemList { get; set; }

        [XmlAttribute("basepath")]
        public String basepath { get; set; }

        [XmlAttribute("referencetype")]
        public String referencetype { get; set; }

        #endregion Properties

        /// <summary>
        /// Diagnostic-method printing Domainmodel data
        /// </summary>
        public void print()
        {
            DomainModelHandler.Instance.loggingDM("--Printing out Problems:");
            DomainModelHandler.Instance.loggingDM("--basepath:" + basepath);
            DomainModelHandler.Instance.loggingDM("--referencetype:" + referencetype);
            foreach (Problem pr in problemList)
            {
                pr.print();
            }
        }
    }

    public class Problem
    {
        #region Properties

        [XmlAttribute("description")]
        public string description { get; set; }
        [XmlAttribute("id")]
        public string id { get; set; }
        [XmlAttribute("title")]
        public string title { get; set; }
        [XmlAttribute("uri")]
        public string uri { get; set; }
        [XmlElement("question")]
        public Question question { get; set; }

        #endregion Properties

        /// <summary>
        /// Diagnostic-method printing Domainmodel data
        /// </summary>
        public void print()
        {
            DomainModelHandler.Instance.loggingDM("---");
            DomainModelHandler.Instance.loggingDM("---description:" + description);
            DomainModelHandler.Instance.loggingDM("---id:" + id);
            DomainModelHandler.Instance.loggingDM("---title:" + title);
            DomainModelHandler.Instance.loggingDM("---uri:" + uri);
            question.print();
        }
    }

    public class Question
    {
        #region Properties

        [XmlAttribute("title")]
        public String title { get; set; }
        [XmlAttribute("type")]
        public String type { get; set; }
        [XmlElement("option")]
        public List<Option> options { get; set; }

        #endregion Properties

        /// <summary>
        /// Diagnostic-method printing Domainmodel data
        /// </summary>
        public void print()
        {
            DomainModelHandler.Instance.loggingDM("----Question:");
            DomainModelHandler.Instance.loggingDM("----title: " + title);
            DomainModelHandler.Instance.loggingDM("----type: " + type);
            DomainModelHandler.Instance.loggingDM("----options: ");
            foreach (Option op in options)
                op.print();
        }
    }

    public class Option
    {
        #region Properties

        [XmlAttribute("correct")]
        public String correct { get; set; }
        [XmlAttribute("title")]
        public String title { get; set; }

        #endregion Properties

        /// <summary>
        /// Diagnostic-method printing Domainmodel data
        /// </summary>
        public void print()
        {
            DomainModelHandler.Instance.loggingDM("-----title:" + title);
            DomainModelHandler.Instance.loggingDM("-----correct:" + correct);
        }
    }

    public class LearningobjectsList
    {
        #region Properties

        [XmlElement("learningobject")]
        public List<Learningobject> learningobjectList { get; set; }

        [XmlAttribute("basepath")]
        public String basepath { get; set; }

        [XmlAttribute("referencetype")]
        public String referencetype { get; set; }

        #endregion Properties

        /// <summary>
        /// Diagnostic-method printing Domainmodel data
        /// </summary>
        public void print()
        {
            DomainModelHandler.Instance.loggingDM("--Printing out Learningobjects:");
            DomainModelHandler.Instance.loggingDM("--basepath:" + basepath);
            DomainModelHandler.Instance.loggingDM("--referencetype:" + referencetype);
            foreach (Learningobject lo in learningobjectList)
            {
                lo.print();
            }
        }
    }

    public class Learningobject
    {
        #region Properties

        [XmlAttribute("description")]
        public string description { get; set; }
        [XmlAttribute("id")]
        public string id { get; set; }
        [XmlAttribute("title")]
        public string title { get; set; }
        [XmlAttribute("uri")]
        public string uri { get; set; }

        #endregion Properties
        #region Constructor

        /// <summary>
        /// default c-tor
        /// </summary>
        public Learningobject() { }

        /// <summary>
        /// c-tor with id
        /// </summary>
        /// <param name="id"> learningobject identifier </param>
        public Learningobject(String id)
        {
            this.id = id;
        }

        #endregion Constructor

        /// <summary>
        /// Diagnostic-method printing Domainmodel data
        /// </summary>
        public void print()
        {
            DomainModelHandler.Instance.loggingDM("---");
            DomainModelHandler.Instance.loggingDM("---description:" + description);
            DomainModelHandler.Instance.loggingDM("---id:" + id);
            DomainModelHandler.Instance.loggingDM("---title:" + title);
            DomainModelHandler.Instance.loggingDM("---uri:" + uri);
        }
    }

    public class CompetenceList
    {
        #region Properties

        [XmlElement("competence")]
        public List<CompetenceDesc> competenceList { get; set; }

        #endregion Properties

        /// <summary>
        /// Diagnostic-method printing Domainmodel data
        /// </summary>
        public void print()
        {
            DomainModelHandler.Instance.loggingDM("--Printing out Competences:");
            foreach (CompetenceDesc comp in competenceList)
                comp.print();
        }
    }

    public class CompetenceDesc
    {

        #region Properties

        [XmlAttribute("description")]
        public string description { get; set; }
        [XmlAttribute("id")]
        public string id { get; set; }
        [XmlAttribute("title")]
        public string title { get; set; }
        [XmlAttribute("uri")]
        public string uri { get; set; }

        #endregion Properties

        #region Constructor

        /// <summary>
        /// default c-tor
        /// </summary>
        public CompetenceDesc() { }

        /// <summary>
        /// c-tor with id
        /// </summary>
        /// <param name="id"> competence identifier </param>
        public CompetenceDesc(String id)
        {
            this.id = id;
        }

        #endregion Constructor

        /// <summary>
        /// Diagnostic-method printing Domainmodel data
        /// </summary>
        public void print()
        {
            DomainModelHandler.Instance.loggingDM("---");
            DomainModelHandler.Instance.loggingDM("---description:" + description);
            DomainModelHandler.Instance.loggingDM("---id:" + id);
            DomainModelHandler.Instance.loggingDM("---title:" + title);
            DomainModelHandler.Instance.loggingDM("---uri:" + uri);
        }
    }

    public class Relations
    {
        #region Properties

        //TODO Liste competencedefinitions
        //TODO Liste competenceprerequisites
        [XmlElement("competenceprerequisites")]
        public CompetenceprerequisitesList competenceprerequisites { get; set; }
        [XmlElement("learningobjects")]
        public LearningobjectsRelationList learningobjects { get; set; }
        [XmlElement("problems")]
        public ProblemsRelationList problems { get; set; }

        #endregion Properties

        /// <summary>
        /// Diagnostic-method printing Domainmodel data
        /// </summary>
        public void print()
        {
            DomainModelHandler.Instance.loggingDM("-Printing out relations:");
            if (competenceprerequisites != null)
                competenceprerequisites.print();
            if (learningobjects != null)
                learningobjects.print();
            if (problems != null)
                problems.print();
        }
    }

    public class ProblemsRelationList
    {
        #region Properties

        [XmlElement("problem")]
        public List<ProblemRelation> problems { get; set; }

        #endregion Properties

        /// <summary>
        /// Diagnostic-method printing Domainmodel data
        /// </summary>
        public void print()
        {
            DomainModelHandler.Instance.loggingDM("--Printing out learningobjects-Relation:");
            foreach (ProblemRelation prr in problems)
            {
                prr.print();
            }
        }
    }

    public class ProblemRelation
    {
        #region Properties

        [XmlAttribute("id")]
        public String id { get; set; }
        [XmlElement("competence")]
        public CompetenceProblem competence { get; set; }

        #endregion Properties

        /// <summary>
        /// Diagnostic-method printing Domainmodel data
        /// </summary>
        public void print()
        {
            DomainModelHandler.Instance.loggingDM("---id pr.:" + id);
            competence.print();
        }
    }

    public class CompetenceProblem
    {
        #region Properties

        [XmlAttribute("id")]
        public String id { get; set; }

        #endregion Properties

        /// <summary>
        /// Diagnostic-method printing Domainmodel data
        /// </summary>
        public void print()
        {
            DomainModelHandler.Instance.loggingDM("---id comp.:" + id);
        }
    }

    public class LearningobjectsRelationList
    {
        #region Properties

        [XmlElement("learningobject")]
        public List<LearningobjectRelation> learningobjects { get; set; }

        #endregion Properties

        /// <summary>
        /// Diagnostic-method printing Domainmodel data
        /// </summary>
        public void print()
        {
            DomainModelHandler.Instance.loggingDM("--Printing out learningobjects-Relation:");
            foreach (LearningobjectRelation lor in learningobjects)
            {
                lor.print();
            }
        }
    }

    public class LearningobjectRelation
    {
        #region Properties

        [XmlAttribute("id")]
        public String id { get; set; }
        [XmlElement("competence")]
        public CompetenceLO competence { get; set; }

        #endregion Properties
        #region Constructors

        /// <summary>
        /// default c-tor
        /// </summary>
        public LearningobjectRelation() { }

        /// <summary>
        /// c-tor with learningobject id and competence id for a competence which is part of the learning object
        /// </summary>
        /// 
        /// <param name="learningobjectId">learningobject id</param>
        /// <param name="competenceId">competence id for a competence which is part of the learning object</param>
        public LearningobjectRelation(String learningobjectId, String competenceId)
        {
            this.id = learningobjectId;
            CompetenceLO clo = new CompetenceLO();
            clo.id = competenceId;
            this.competence = clo;
        }

        #endregion Constructors

        /// <summary>
        /// Diagnostic-method printing Domainmodel data
        /// </summary>
        public void print()
        {
            DomainModelHandler.Instance.loggingDM("---id lo.:" + id);
            competence.print();
        }
    }

    public class CompetenceLO
    {

        #region Properties

        [XmlAttribute("id")]
        public String id { get; set; }

        #endregion Properties

        /// <summary>
        /// Diagnostic-method printing Domainmodel data
        /// </summary>
        public void print()
        {
            DomainModelHandler.Instance.loggingDM("---id comp.:" + id);
        }
    }

    public class CompetenceprerequisitesList
    {
        #region Properties

        [XmlElement("competence")]
        public List<CompetenceP> competences { get; set; }

        #endregion Properties

        /// <summary>
        /// Diagnostic-method printing Domainmodel data
        /// </summary>
        public void print()
        {
            DomainModelHandler.Instance.loggingDM("--Printing out Competenceprerequisites");
            foreach (CompetenceP cp in competences)
            {
                cp.print();
            }
        }
    }

    public class CompetenceP
    {
        #region Properties

        [XmlAttribute("id")]
        public String id { get; set; }
        [XmlElement("prereqcompetence")]
        public Prereqcompetence prereqcompetence { get; set; }

        #endregion Properties

        #region Constructors

        /// <summary>
        /// default c-tor
        /// </summary>
        public CompetenceP() { }

        /// <summary>
        /// modified c-tor
        /// </summary>
        /// 
        /// <param name="id"> id of the competence </param>
        /// <param name="prerequisiteID"> id of the prerequisite competence </param>
        public CompetenceP(String id, String prerequisiteID)
        {
            this.id = id;
            this.prereqcompetence = new Prereqcompetence();
            this.prereqcompetence.id = prerequisiteID;
        }

        #endregion Constructors

        /// <summary>
        /// Diagnostic-method printing Domainmodel data
        /// </summary>
        public void print()
        {
            DomainModelHandler.Instance.loggingDM("---id: " + id);
            prereqcompetence.print();
        }
    }

    public class Prereqcompetence
    {
        #region Properties

        [XmlAttribute("id")]
        public String id { get; set; }

        #endregion Properties

        /// <summary>
        /// Diagnostic-method printing Domainmodel data
        /// </summary>
        public void print()
        {
            DomainModelHandler.Instance.loggingDM("---id prereq.:" + id);
        }
    }

    #endregion Serializing 
}
