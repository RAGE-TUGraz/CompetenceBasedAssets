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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
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
        /// Run-time Asset storage of domain model.
        /// </summary>
        private DomainModel domainModel = null;

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
        //TODO: default domain model loading behaviour
        #region Methods 

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
        /// Method returning domain model either from the run-tima asset storage if available or from specified (default) source(File/Web).
        /// </summary>
        /// 
        /// <returns> The domein model. </returns>
        internal DomainModel getDomainModel()
        {
            if (domainModel != null)
                return domainModel;
            DomainModel dm = loadDefaultDomainModel();
            domainModel = dm;
            return dm;
        }

        
        /// <summary>
        /// Method for storing a domain model.
        /// </summary>
        /// <param name="dm"> Domain model to store. </param>
        internal void storeDomainModel(DomainModel dm)
        {
            domainModel = dm;
        }
        

        /// <summary>
        /// Method loading domain model - location specified by settings.
        /// </summary>
        /// <returns>Domain Model for the player.</returns>
        internal DomainModel loadDefaultDomainModel()
        {
            loggingDM("Loading default Domain model.");
            DomainModelAssetSettings dmas = getDMA().getSettings();

            if (dmas.LocalSource)
            {
                IDataStorage ids = (IDataStorage)AssetManager.Instance.Bridge;
                if (ids != null )
                {
                    if (!ids.Exists(dmas.Source))
                    {
                        loggingDM("File "+ dmas.Source + " not found for loading Domain model.", Severity.Error);
                        throw new Exception("EXCEPTION: File "+ dmas.Source + " not found for loading Domain model.") ;
                    }

                    loggingDM("Loading DomainModel from File.");
                    return (this.getDMFromXmlString(ids.Load(dmas.Source)));
                }
                else
                {
                    loggingDM("IDataStorage bridge absent for requested local loading method of the Domain model.", Severity.Error);
                    throw new Exception("EXCEPTION: IDataStorage bridge absent for requested local loading method of the Domain model.");
                }
            }
            else
            {
                IWebServiceRequest iwr = (IWebServiceRequest)AssetManager.Instance.Bridge;
                if (iwr != null)
                {
                    loggingDM("Loading web DomainModel.");
                    Uri uri = new Uri(dmas.Source);
                    Dictionary<string, string> headers = new Dictionary<string, string>();
                    //headers.Add("user", playerId);
                    string body = dmas.Source;
                    WebServiceResponse wsr = new WebServiceResponse();
                    //currentPlayerId = playerId;
                    iwr.WebServiceRequest("get", uri, headers, body, wsr);
                    //currentPlayerId = null;

                    return (domainModel);
                }
                else
                {
                    loggingDM("IWebServiceRequest bridge absent for requested web loading method of the Domain model.", Severity.Error);
                    throw new Exception("EXCEPTION: IWebServiceRequest bridge absent for requested web loading method of the Domain model.");
                }
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
        

        #endregion Methods
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
            //performTest2();
            performTest3();
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

            DomainModelAssetSettings oldDMAS = getDMA().getSettings();
            DomainModelAssetSettings newDMAS = new DomainModelAssetSettings();
            newDMAS.LocalSource = true;
            newDMAS.Source = fileId;

            getDMA().Settings = newDMAS;

            DomainModel dm2 = loadDefaultDomainModel();

            if (!dm.toXmlString().Equals(dm2.toXmlString()))
            {
                loggingDM("DomainModelAsset - Test1 failed!", Severity.Error);
                throw new Exception("EXCEPTION: DomainModelAsset - Test1 failed!");
            }
            else
            {
                loggingDM("DomainModelAsset - Test1 passed.");
            }

            getDMA().Settings = oldDMAS;
        }

        /// <summary>
        /// Loads web-domain model.
        /// </summary>
        /*
        private void performTest2()
        {
            DomainModelAssetSettings oldDMAS = getDMA().getSettings();
            DomainModelAssetSettings dmas = new DomainModelAssetSettings();
            dmas.WebSource = true;
            dmas.Source = @"http://css-kmi.tugraz.at:8080/compod/rest/getdomainmodel?id=isr2013";
            getDMA().Settings = dmas;

            try
            {
                DomainModel dm = getDMA().getDomainModel();
            }
            catch(Exception e)
            {
                loggingDM(e.Message);
                loggingDM("Maybe the uri ("+dmas.Source+") is not valid any more.");
            }

            getDMA().Settings = oldDMAS;
        }
        */

        /// <summary>
        /// Loading and printing local domain model
        /// </summary>
        private void performTest3()
        {
            DomainModelAssetSettings oldDMAS = getDMA().getSettings();
            DomainModelAssetSettings dmas = new DomainModelAssetSettings();
            dmas.WebSource = false;
            dmas.Source = "dmNew.xml";
            getDMA().Settings = dmas;

            try
            {
                DomainModel dm = getDMA().getDomainModel();
                dm.print();
            }
            catch (Exception e)
            {
                loggingDM("Loading of domain model 'dmNew.xml' not possible.");
                loggingDM(e.Message);
            }

            getDMA().Settings = oldDMAS;
        }

        /// <summary>
        /// Method creating an example domain model
        /// </summary>
        /// <returns></returns>
        public DomainModel createExampleDomainModel()
        {

            DomainModel dm = new DomainModel();


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
            SituationsList sl = new SituationsList();
            Situation s1 = new Situation("gs1");
            Situation s2 = new Situation("gs2");
            Situation s3 = new Situation("gs3");
            Situation s4 = new Situation("gs4");
            Situation s5 = new Situation("gs5");
            Situation s6 = new Situation("gs6");
            Situation s7 = new Situation("gs7");
            Situation s8 = new Situation("gs8");
            Situation s9 = new Situation("gs9");
            Situation s10 = new Situation("gs10");
            Situation[] sArray = { s1, s2, s3, s4, s5, s6, s7, s8, s9, s10 };
            List<Situation> loList = new List<Situation>(sArray);
            sl.situationList = loList;
            elements.situations = sl;

            //Competences prerequisites
            Relations relations = new Relations();
            CompetenceprerequisitesList cpl = new CompetenceprerequisitesList();
            CompetenceP cp1 = new CompetenceP("C5", new String[] { "C1", "C2" });
            CompetenceP cp3 = new CompetenceP("C6", new String[] { "C4" });
            CompetenceP cp4 = new CompetenceP("C7", new String[] { "C4" });
            CompetenceP cp5 = new CompetenceP("C8", new String[] { "C3", "C6" });
            CompetenceP cp7 = new CompetenceP("C9", new String[] { "C5", "C8" });
            CompetenceP cp8 = new CompetenceP("C10", new String[] { "C9", "C7" });
            CompetenceP[] cpArray = { cp1, cp3, cp4, cp5, cp7, cp8 };
            List<CompetenceP> cpList = new List<CompetenceP>(cpArray);
            cpl.competences = cpList;
            relations.competenceprerequisites = cpl;

            //assignmend of competences to game situations (=learning objects)
            SituationRelationList lorl = new SituationRelationList();
            SituationRelation lor1 = new SituationRelation("gs1", new String[] { "C1" });
            SituationRelation lor2 = new SituationRelation("gs2", new String[] { "C2" });
            SituationRelation lor3 = new SituationRelation("gs3", new String[] { "C3" });
            SituationRelation lor4 = new SituationRelation("gs4", new String[] { "C4" });
            SituationRelation lor5 = new SituationRelation("gs5", new String[] { "C5", "C1", "C2" });
            SituationRelation lor8 = new SituationRelation("gs6", new String[] { "C6", "C4" });
            SituationRelation lor10 = new SituationRelation("gs7", new String[] { "C4", "C7" });
            SituationRelation lor12 = new SituationRelation("gs8", new String[] { "C8", "C6", "C3" });
            SituationRelation lor15 = new SituationRelation("gs9", new String[] { "C9", "C5", "C8" });
            SituationRelation lor18 = new SituationRelation("gs10", new String[] { "C10", "C9", "C7" });
            SituationRelation[] lorArray = { lor1, lor2, lor3, lor4, lor5, lor8, lor10, lor12, lor15, lor18 };
            List<SituationRelation> lorList = new List<SituationRelation>(lorArray);
            lorl.situations = lorList;
            relations.situations = lorl;

            dm.elements = elements;
            dm.relations = relations;

            //Update Levels
            UpdateLevel ul1 = new UpdateLevel();
            ul1.direction = "up";
            ul1.power = "low";
            ul1.xi = "1.2";
            ul1.minonecompetence = "false";
            ul1.maxonelevel = "true";
            UpdateLevel ul2 = new UpdateLevel();
            ul2.direction = "up";
            ul2.power = "medium";
            ul2.xi = "2";
            ul2.minonecompetence = "false";
            ul2.maxonelevel = "true";
            UpdateLevel ul3 = new UpdateLevel();
            ul3.direction = "up";
            ul3.power = "high";
            ul3.xi = "4";
            ul3.minonecompetence = "true";
            ul3.maxonelevel = "false";
            UpdateLevel ul4 = new UpdateLevel();
            ul4.direction = "down";
            ul4.power = "low";
            ul4.xi = "1.2";
            ul4.minonecompetence = "false";
            ul4.maxonelevel = "true";
            UpdateLevel ul5 = new UpdateLevel();
            ul5.direction = "down";
            ul5.power = "medium";
            ul5.xi = "2";
            ul5.minonecompetence = "false";
            ul5.maxonelevel = "true";
            UpdateLevel ul6 = new UpdateLevel();
            ul6.direction = "down";
            ul6.power = "high";
            ul6.xi = "4";
            ul6.minonecompetence = "true";
            ul6.maxonelevel = "false";
            UpdateLevel[] ulArray = { ul1, ul2, ul3, ul4, ul5, ul6 };
            dm.updateLevels = new UpdateLevels();
            dm.updateLevels.updateLevelList = new List<UpdateLevel>(ulArray);

            return dm;
        }
        

        #endregion TestMethods

    }

    /// <summary>
    /// Implementation of the WebServiceResponse-Interface for handling web requests.
    /// </summary>
    public class WebServiceResponse : IWebServiceResponse
    {
        /// <summary>
        /// Describes behaviour in case the web request failed.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="msg"></param>
        public void Error(string url, string msg)
        {
            DomainModelHandler.Instance.loggingDM("Web Request for retriving Domain model from "+url+" failed! " + msg, Severity.Error);
            throw new Exception("EXCEPTION: Web Request for retriving Domain model from " + url + " failed! " + msg);
        }

        /// <summary>
        /// Describes behaviour in case the web requests succeeds
        /// </summary>
        /// <param name="url"></param>
        /// <param name="code"></param>
        /// <param name="headers"></param>
        /// <param name="body"></param>
        public void Success(string url, int code, Dictionary<string, string> headers, string body)
        {
            DomainModelHandler.Instance.loggingDM("WebClient request successful!");
            DomainModelHandler.Instance.storeDomainModel(DomainModelHandler.Instance.getDMFromXmlString(body));
        }
    }

    /// <summary>
    /// Classes for Serializing/Deserializing Domainmodels (e.g. DM like http://css-kmi.tugraz.at:8080/compod/rest/getdomainmodel?id=isr2013)
    /// </summary>
    /*
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
    */
    #region Serializing 

    /// <summary>
    /// Class containing all Domainmodel data.
    /// </summary>
    [XmlRoot("domainmodel")]
    public class DomainModel
    {
        #region Properties
        
        [XmlElement("elements")]
        public Elements elements { get; set; }

        [XmlElement("relations")]
        public Relations relations { get; set; }
        
        [XmlElement("updatelevels")]
        public UpdateLevels updateLevels { get; set; }


        #endregion Properties

        /// <summary>
        /// Diagnostic-method printing Domainmodel data
        /// </summary>
        public void print()
        {
            DomainModelHandler.Instance.loggingDM("Printing out DM:");
            elements.print();
            relations.print();
            updateLevels.print();
        }

        #region Methods

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

    public class UpdateLevels
    {
        #region Fields

        [XmlElement("level")]
        public List<UpdateLevel> updateLevelList { get; set; }
       
        #endregion Fields
        #region Methods

        public void print()
        {
             DomainModelHandler.Instance.loggingDM("-Printing out updateLevels:");
             foreach(UpdateLevel ul in updateLevelList)
                ul.print();
        }

        #endregion Methods
    }

    public class UpdateLevel
    {

        #region Fields

        [XmlAttribute("direction")]
        public string direction { get; set; }

        [XmlAttribute("power")]
        public string power { get; set; }

        [XmlAttribute("xi")]
        public string xi { get; set; }

        [XmlAttribute("minonecompetence")]
        public string minonecompetence { get; set; }

        [XmlAttribute("maxonelevel")]
        public string maxonelevel { get; set; }

        #endregion Fields


        #region Methods

        public void print()
        {
            DomainModelHandler.Instance.loggingDM("-----");
            DomainModelHandler.Instance.loggingDM("--direction:" +direction);
            DomainModelHandler.Instance.loggingDM("--power:" + power);
            DomainModelHandler.Instance.loggingDM("--xi:" + xi);
            DomainModelHandler.Instance.loggingDM("--minonecompetence:" + minonecompetence);
            DomainModelHandler.Instance.loggingDM("--maxonelevel:" + maxonelevel);

        }

        #endregion Methods

    }
    
    public class Elements
    {
        #region Properties
        
        [XmlElement("competences")]
        public CompetenceList competences { get; set; }
        [XmlElement("situations")]
        public SituationsList situations { get; set; }
        [XmlElement("activities")]
        public ActivityList activities { get; set; }

        #endregion Properties

        /// <summary>
        /// Diagnostic-method printing Domainmodel data
        /// </summary>
        public void print()
        {
            DomainModelHandler.Instance.loggingDM("-Printing out elements:");
            if (competences != null)
                competences.print();
            if (situations != null)
                situations.print();
            if (activities != null)
                activities.print();
        }
    }

    public class ActivityList
    {
        #region Properties

        [XmlElement("activity")]
        public List<Activity> activityList { get; set; }
        
        #endregion Properties

        /// <summary>
        /// Diagnostic-method printing Domainmodel data
        /// </summary>
        public void print()
        {
            DomainModelHandler.Instance.loggingDM("--Printing out Activities:");
            foreach (Activity ac in activityList)
            {
                ac.print();
            }
        }
    }

    public class Activity
    {
        #region Properties
    
        [XmlAttribute("id")]
        public string id { get; set; }

        #endregion Properties
        #region Constructors

        public Activity() { }

        public Activity(string id)
        {
            this.id = id;
        }

        #endregion Constructors
        #region Methods
        /// <summary>
        /// Diagnostic-method printing Domainmodel data
        /// </summary>
        public void print()
        {
            DomainModelHandler.Instance.loggingDM("---id:" + id);
        }
        #endregion Methods
    }
    
    public class SituationsList
    {
        #region Properties

        [XmlElement("situation")]
        public List<Situation> situationList { get; set; }
        
        #endregion Properties

        /// <summary>
        /// Diagnostic-method printing Domainmodel data
        /// </summary>
        public void print()
        {
            DomainModelHandler.Instance.loggingDM("--Printing out Situations:");
            foreach (Situation si in situationList)
            {
                si.print();
            }
        }
    }

    public class Situation
    {
        #region Properties

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
        public Situation() { }

        /// <summary>
        /// c-tor with id
        /// </summary>
        /// <param name="id"> Situation identifier </param>
        public Situation(String id)
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
    
        [XmlElement("competenceprerequisites")]
        public CompetenceprerequisitesList competenceprerequisites { get; set; }

        [XmlElement("situations")]
        public SituationRelationList situations { get; set; }
        [XmlElement("activities")]
        public ActivityRelationList activities { get; set; }

        #endregion Properties

        /// <summary>
        /// Diagnostic-method printing Domainmodel data
        /// </summary>
        public void print()
        {
            DomainModelHandler.Instance.loggingDM("-Printing out relations:");
            if (competenceprerequisites != null)
                competenceprerequisites.print();
            if (situations != null)
                situations.print();
            if (activities != null)
                activities.print();
        }
    }

    public class SituationRelationList
    {
        #region Properties

        [XmlElement("situation")]
        public List<SituationRelation> situations { get; set; }

        #endregion Properties

        /// <summary>
        /// Diagnostic-method printing Domainmodel data
        /// </summary>
        public void print()
        {
            DomainModelHandler.Instance.loggingDM("--Printing out situation-Relation:");
            foreach (SituationRelation sr in situations)
            {
                sr.print();
            }
        }

        /*
        public void addSituationRelation(string situationId, string competenceId)
        {
            Boolean found = false;
            foreach(SituationRelation sr in this.situations)
            {
                if (sr.id.Equals(situationId))
                {
                    CompetenceSituation competence = new CompetenceSituation();
                    competence.id = competenceId;
                    competence.up = "medium";
                    competence.down = "medium";
                    sr.competences.Add(competence);
                    found = true;
                    break;
                }
            }
            if (!found)
                situations.Add(new SituationRelation(situationId,new String[] { competenceId }));
        }
        */
    }

    public class SituationRelation
    {
        #region Properties

        [XmlAttribute("id")]
        public String id { get; set; }
        [XmlElement("competence")]
        public List<CompetenceSituation> competences { get; set; }

        #endregion Properties
        #region Constructors

        public SituationRelation() { }

        public SituationRelation(string situationId, string[] competenceId)
        {
            id = situationId;
            competences = new List<CompetenceSituation>();
            foreach(String cid in competenceId)
            {
                CompetenceSituation competence = new CompetenceSituation();
                competence.id = cid;
                competence.up = "medium";
                competence.down = "medium";
                competences.Add(competence);
            }
        }

        #endregion Constructors
        #region Methds
        /// <summary>
        /// Diagnostic-method printing Domainmodel data
        /// </summary>
        public void print()
        {
            DomainModelHandler.Instance.loggingDM("---------------");
            DomainModelHandler.Instance.loggingDM("---id pr.:" + id);
            foreach(CompetenceSituation cs in this.competences)
                cs.print();
        }

        #endregion Methods
    }

    public class CompetenceSituation
    {
        #region Properties

        [XmlAttribute("id")]
        public String id { get; set; }
        
        [XmlAttribute("levelup")]
        public String up { get; set; }
        
        [XmlAttribute("leveldown")]
        public String down { get; set; }

        #endregion Properties

        /// <summary>
        /// Diagnostic-method printing Domainmodel data
        /// </summary>
        public void print()
        {
            DomainModelHandler.Instance.loggingDM("---id comp.:" + id);
            DomainModelHandler.Instance.loggingDM("---up comp.:" + up);
            DomainModelHandler.Instance.loggingDM("---down comp.:" + down);
        }
    }

    public class ActivityRelationList
    {
        #region Properties

        [XmlElement("activity")]
        public List<ActivitiesRelation> activities { get; set; }

        #endregion Properties

        /// <summary>
        /// Diagnostic-method printing Domainmodel data
        /// </summary>
        public void print()
        {
            DomainModelHandler.Instance.loggingDM("--Printing out activities-Relation:");
            foreach (ActivitiesRelation ar in activities)
            {
                ar.print();
            }
        }

        /*
        public void addActivityRelation(String activitytId, String competenceId)
        {
            Boolean found = false;
            foreach(ActivitiesRelation ar in this.activities)
                if(ar.id == activitytId)
                {
                    CompetenceActivity clo = new CompetenceActivity();
                    clo.id = competenceId;
                    ar.competences.Add(clo);
                    found = true;
                    break;
                }
            if (!found)
                this.activities.Add(new ActivitiesRelation(activitytId, competenceId ));
        }
        */
    }

    public class ActivitiesRelation
    {
        #region Properties

        [XmlAttribute("id")]
        public String id { get; set; }
        [XmlElement("competence")]
        public List<CompetenceActivity> competences { get; set; }

        #endregion Properties
        #region Constructors

        /// <summary>
        /// default c-tor
        /// </summary>
        public ActivitiesRelation() { }

        /// <summary>
        /// c-tor with learningobject id and competence id for a competence which is part of the learning object
        /// </summary>
        /// 
        /// <param name="activitytId">activity id</param>
        /// <param name="competenceId">competence id for a competence which is part of the learning object</param>
        public ActivitiesRelation(String activitytId, CompetenceActivity[] competenceId)
        {
            this.id = activitytId;
            this.competences = new List<CompetenceActivity>();
            foreach(CompetenceActivity cid in competenceId)
                this.competences.Add(cid);
        }

        public ActivitiesRelation(String id, string competenceId)
        {
            this.id = id;
            competences = new List<CompetenceActivity>();
            competences.Add(new CompetenceActivity(competenceId,"medium","up"));
        }

        #endregion Constructors

        /// <summary>
        /// Diagnostic-method printing Domainmodel data
        /// </summary>
        public void print()
        {
            DomainModelHandler.Instance.loggingDM("---id lo.:" + id);
            foreach(CompetenceActivity ca in this.competences)
                ca.print();
        }
    }

    public class CompetenceActivity
    {

        #region Properties

        [XmlAttribute("id")]
        public String id { get; set; }

        [XmlAttribute("power")]
        public String power { get; set; }

        [XmlAttribute("direction")]
        public String direction { get; set; }

        #endregion Properties
        #region Constructors

        public CompetenceActivity() { }

        public CompetenceActivity(string id, string power, string direction)
        {
            this.id = id;
            this.power = power;
            this.direction = direction;
        }

        #endregion Constructors
        #region Methods
        /// <summary>
        /// Diagnostic-method printing Domainmodel data
        /// </summary>
        public void print()
        {
            DomainModelHandler.Instance.loggingDM("---id comp.:" + id);
            DomainModelHandler.Instance.loggingDM("---power comp.:" + power);
            DomainModelHandler.Instance.loggingDM("---direction comp.:" + direction);
        }
        #endregion Methods
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

        /*
        public void addPrerequisiteCompetence(String id, String prerequisiteID)
        {
            Boolean found = false;
            foreach(CompetenceP cp in this.competences)
            {
                if (cp.id.Equals(id))
                {
                    Prereqcompetence precom = new Prereqcompetence();
                    precom.id = prerequisiteID;
                    cp.prereqcompetences.Add(precom);
                    found = true;
                    break;
                }
            }
            if (!found)
                competences.Add(new CompetenceP(id, new String[] { prerequisiteID }));
        }
        */
    }

    public class CompetenceP
    {
        #region Properties

        [XmlAttribute("id")]
        public String id { get; set; }
        [XmlElement("prereqcompetence")]
        public List<Prereqcompetence> prereqcompetences { get; set; }

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
        public CompetenceP(String id, String[] prerequisiteIDs)
        {
            this.id = id;
            prereqcompetences = new List<Prereqcompetence>();
            foreach (String pid in prerequisiteIDs)
            {
                Prereqcompetence precom = new Prereqcompetence();
                precom.id = pid;
                prereqcompetences.Add(precom);
            }
        }

        #endregion Constructors

        /// <summary>
        /// Diagnostic-method printing Domainmodel data
        /// </summary>
        public void print()
        {
            DomainModelHandler.Instance.loggingDM("---id: " + id);
            foreach(Prereqcompetence pc in prereqcompetences)
                pc.print();
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
