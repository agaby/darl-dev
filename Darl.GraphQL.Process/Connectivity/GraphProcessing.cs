using Darl.GraphQL.Models.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Darl.GraphQL.Models.Connectivity
{
    public class GraphProcessing : IGraphProcessing
    {
        /// <summary>
        /// Create a graph connection
        /// </summary>
        /// <param name="userId">The user</param>
        /// <param name="graphConnection">The connection description</param>
        /// <param name="definitive">if false check for ontological compliance and throw an exception if non-compliant, if true force the addition </param>
        /// <returns></returns>
        public Task<GraphConnection> CreateGraphConnection(string userId, GraphConnectionInput graphConnection, bool definitive = false)
        {
            if(!definitive)//ontological compliance checks
            {
                //Look for a preceding and a following association in this or higher verbs that permits this.
                //This can be written as a gremlin query
                //if no path found throw ExecutionError 
                //for each property lineage 
                //Look for a preceding and a following association in the verb 'has' that permits this.
                //This can be written as a gremlin query
                //if no path found throw ExecutionError 
            }
            throw new NotImplementedException();
        }

        /// <summary>
        /// Create a graph object
        /// </summary>
        /// <param name="userId">The user</param>
        /// <param name="graphObject">The object description</param>
        /// <param name="definitive">if false check for ontological compliance and throw an exception if non-compliant, if true force the addition </param>
        /// <returns></returns>
        public Task<GraphObject> CreateGraphObject(string userId, GraphObjectInput graphObject, bool definitive = false)
        {
            if (!definitive)//ontological compliance checks
            {
                //for each property lineage 
                //Look for a preceding and a following association in the verb 'has' that permits this.
                //This can be written as a gremlin query
                //if no path found throw ExecutionError 
            }
            throw new NotImplementedException();
        }

        /// <summary>
        /// Delete a connection
        /// </summary>
        /// <param name="userId">The user's id</param>
        /// <param name="id">The id of the connection to delete</param>
        /// <returns></returns>
        public Task<GraphConnection> DeleteGraphConnection(string userId, string id)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Delete an object
        /// </summary>
        /// <param name="userId">The user's id</param>
        /// <param name="id">The object's id</param>
        /// <returns></returns>
        public Task<GraphObject> DeleteGraphObject(string userId, string id)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Get a graph object by the id
        /// </summary>
        /// <param name="userId">The user's id</param>
        /// <param name="id">The object's id</param>
        /// <returns>The object</returns>
        public Task<GraphObject> GetGraphObjectById(string userId, string id)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Get graph objects with an exact name match
        /// </summary>
        /// <param name="userId">The user's id</param>
        /// <param name="name">The name</param>
        /// <param name="lineage">The lineage of the object</param>
        /// <returns></returns>
        public Task<List<GraphObject>> GetGraphObjects(string userId, string name, string lineage)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Get graph objects with a fuzzy name match
        /// </summary>
        /// <param name="userId">The user's id</param>
        /// <param name="name">The name to fuzzy match</param>
        /// <param name="lineage">The kind of the object</param>
        /// <param name="distance">The max Levenshtein distance of a match</param>
        /// <returns></returns>
        public Task<List<GraphObject>> GetGraphObjectsFuzzy(string userId, string name, string lineage, float distance)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Update a graph connection
        /// </summary>
        /// <param name="userId">The user's id</param>
        /// <param name="graphConnection">The connection definition - all included fields are updated</param>
        /// <param name="definitive">if false check for ontological compliance and throw an exception if non-compliant, if true force the addition </param>
        /// <returns></returns>
        public Task<GraphConnection> UpdateGraphConnection(string userId, GraphConnectionUpdate graphConnection, bool definitive = false)
        {
            if (!definitive)//ontological compliance checks
            {
                //
                //for each property lineage 
                //Look for a preceding and a following association in the verb 'has' that permits this.
                //This can be written as a gremlin query
                //if no path found throw ExecutionError 
            }
            throw new NotImplementedException();
        }

        /// <summary>
        /// Update a graph object
        /// </summary>
        /// <param name="userId">The user's id</param>
        /// <param name="graphObject">The object definition - all included fields are updated</param>
        /// <param name="definitive">if false check for ontological compliance and throw an exception if non-compliant, if true force the addition </param>
        /// <returns></returns>
        public Task<GraphObject> UpdateGraphObject(string userId, GraphObjectUpdate graphObject, bool definitive = false)
        {
            if (!definitive)//ontological compliance checks
            {
                //for each property lineage 
                //Look for a preceding and a following association in the verb 'has' that permits this.
                //This can be written as a gremlin query
                //if no path found throw ExecutionError 
            }
            throw new NotImplementedException();
        }


    }
}
