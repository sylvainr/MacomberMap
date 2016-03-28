using MacomberMapCommunications.Messages;
using MacomberMapCommunications.Messages.Communications;
using MacomberMapCommunications.Messages.EMS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using MacomberMapCommunications.Messages.Display;

namespace MacomberMapCommunications.WCF
{
    /// <summary>
    /// © 2014, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved.
    /// This class provides the interface to the Oracle back-end system, including models, one-lines, notes, etc.
    /// </summary>
    [ServiceContract]
    public interface IMM_Database_Types
    {
        /// <summary>
        /// Get the most recent date/time stamp on our notes
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        DateTime GetMostRecentNoteDate();

        /// <summary>
        /// Load in our collection of notes
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        MM_Note[] LoadNotes();

        /// <summary>
        /// Upload a note, and return the ID for the note
        /// </summary>
        /// <param name="Note"></param>
        /// <returns></returns>
        [OperationContract]
        int UploadNote(MM_Note Note);

        /// <summary>
        /// Get the level information on the training program
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        MM_Training_Level[] LoadTrainingLevels();

        /// <summary>
        /// Load in our collection of database models
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        MM_Database_Model[] LoadDatabaseModels();

        /// <summary>
        /// Load our model information
        /// </summary>
        /// <param name="Model"></param>
        /// <returns></returns>
        [OperationContract]
        byte[] LoadModel(MM_Database_Model Model);


        /// <summary>
        /// Start a training session, and return the unique ID for the training session
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        int StartTrainingSession();

        /// <summary>
        /// Update a parameter on our training session
        /// </summary>
        /// <param name="Title"></param>
        /// <param name="NewValue"></param>
        /// <param name="SessionId"></param>
        [OperationContract]
        bool UpdateTrainingInformation(String Title, float NewValue, int SessionId);

        /// <summary>
        /// Load the Macomber Map configuration
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        string LoadMacomberMapConfiguration();

        /// <summary>
        /// Load one-line data, and send to 
        /// </summary>
        /// <param name="ElementName"></param>
        /// <param name="ElementType"></param>        
        /// <returns></returns>
        [OperationContract]
        MM_OneLine_Data LoadOneLineData(String ElementName, MM_OneLine_Data.enumElementType ElementType);

        /// <summary>
        /// Propose a coordinate update, storing in the database
        /// </summary>
        /// <param name="Suggestions"></param>
        /// <returns></returns>
        [OperationContract]
        bool PostCoordinateSuggestions(MM_Coordinate_Suggestion[] Suggestions);

        /// <summary>
        /// Update our unit control status
        /// </summary>
        /// <param name="Status"></param>
        /// <returns></returns>
        [OperationContract]
        bool UpdateUnitControlStatusInformation(MM_Unit_Control_Status Status);
    }
}
