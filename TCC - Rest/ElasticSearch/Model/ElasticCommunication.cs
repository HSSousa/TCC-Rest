using Nest;
using System;
using System.Collections.Generic;
using System.Text;

namespace ElasticSearch.Model
{
    public class ElasticCommunication
    {
        private static ElasticClient ElasticClient;
        public ElasticClient GetElasticClient(string index)
        {
            ConnectionSettings settings = null;

            if (ElasticClient == null)
            {
                var node = new Uri("http://localhost:9200");

                settings = new ConnectionSettings(node).DefaultIndex(index);
            }
            return ElasticClient = new ElasticClient(settings);
        }
    }
}
