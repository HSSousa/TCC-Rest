using ClassesDAO.DAO;
using ElasticSearch.Model;
using Nest;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TCC___Implementation.Exceptions;
using TCC___Implementation.Interface;
using TCC___Implementation.Provider;
using TCC___Implementation.Service;

namespace ElasticSearch.Service
{
    public class ElasticSearchService<T> : ICommunication
    {
        private static ElasticCommunication ElasticCommunication;
        private static string Index;
        private static ElasticClient ElasticClient;
        public static bool Initialized = false;

        public ElasticSearchService(string index)
        {
            if (index != null)Index = index;
            if (ElasticClient == null)
            {
                ElasticClient = GetElasticClient();
            }
        }

        public void Inicializacao()
        {
            Initialized = true;
                SerialPort serialPort = new SerialPort("COM16", 9600, Parity.None, 8, StopBits.One);
                try
                {
                    serialPort.Open();
                    serialPort.Write("$H\r\n");
                    Thread.Sleep(2000);
                    serialPort.Write("G10 P0 L20 X0 Y0 Z0\r\n");
                    Thread.Sleep(100);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                finally
                {
                    serialPort.Close();
                }
        }
        public ElasticClient GetElasticClient()
        {
            ElasticCommunication = new ElasticCommunication();
            return ElasticCommunication.GetElasticClient(Index);
        }
        public void CheckIndex(object obj, string indexname)
        {
            if (IndexExists(indexname)) return;
            else
            {
                if (obj is ProductDAO)
                {
                    var response = ElasticClient.Indices.Create(indexname,
                    index => index.Map<ProductDAO>(x => x.AutoMap()));
                }
            }
        }


        public List<BoolResponseDAO> DeleteById(string id, out string erro)
        {
            erro = string.Empty;
            var produtos = this.GetAll();
            bool productExists = false;
            List<BoolResponseDAO> lista = new List<BoolResponseDAO>();
            foreach (var item in produtos)
            {
                if (item.Id == id) productExists = true;
            }
            if (productExists)
            {
                var response = ElasticClient.Delete<Object>(id);
                if (!(response.Result == Result.Deleted))
                {

                    lista.Add(new BoolResponseDAO { Response = false });
                    return lista;
                }
                else
                {
                    lista.Add(new BoolResponseDAO { Response = true });
                    return lista;
                }
            }
            else
            {
                erro = "Produto não encontrado";
                lista.Add(new BoolResponseDAO { Response = false });
                return lista;
            }
        }

        public bool DocumentExists(string id)
        {
            var searchResponse = ElasticClient.Get<Object>(id);
            if (!searchResponse.Found) return false;
            return true;
        }

        public List<ProductDAO> GetAll()
        {
            var searchResponse = ElasticClient.Search<ProductDAO>(s => s.Query(q => q.MatchAll()));
            var quantidade = searchResponse.Total;
            if (quantidade > 0)
            {
                List<ProductDAO> productDAOs = new List<ProductDAO>();
                foreach (var item in searchResponse.Documents)
                {
                    productDAOs.Add(item);
                }
                var ascendingOrder = productDAOs.OrderBy(i => i.Position).ToList();
                return ascendingOrder;
            }
            else
            {
                return  new List<ProductDAO>();
                 
            }
            
        }




        public ProductDAO GetProductById(string id)
        {
            var result = ElasticClient.Get<ProductDAO>(id);
            if (result.Found) return result.Source;
            else throw new ProductNotFoundException();
        }
        public void IndexDocument(object obj, out string erro)
        {
            erro = string.Empty;
            if (obj is ProductDAO)
            {
                var doc = obj as ProductDAO;
                if(doc.Position > 4)
                {
                    erro = "Posição inválida";
                    return;
                }
                if (!DocumentExists(doc.Id))
                {
                    var produtos = this.GetAll();
                    foreach (var item in produtos)
                    {
                        if (item.Position == doc.Position)
                        {
                            erro = "Posição já ocupada por outro produto";
                            return ;
                        }
                    }
                    foreach (var item in produtos)
                    {
                        if(item.Color == doc.Color)
                        {
                            erro = "Já existe um produto com essa cor";
                            return;
                        }
                    }
                    this.CheckIndex(obj, "product");
                    ElasticClient.IndexDocument<object>(obj);
                    return ;
                }
                else
                {
                    erro = "Produto já existe";
                    return ;
                }
            }
            else
            {
                erro = "Objeto não é um produto";
                return ;
            }
        }
        public bool IndexExists(string index)
        {
            var request = new IndexExistsRequest(index);
            return ElasticClient.Indices.Exists(request).Exists;
        }

        public void Validate(string idProduct)
        {
            if (Initialized == false)this.Inicializacao();

            var response = ElasticClient.Get<ProductDAO>(idProduct);
            var product = response.Source;
            if (product.Position == 1)
            {
                this.ExecuteCommand("COM16", new CoordinateDAO() { X = 427M, Y = 147M, Z = 0M }, "1800");
            }
            else if (product.Position == 2)
            {
                this.ExecuteCommand("COM16", new CoordinateDAO() { X = 251M, Y = 147M, Z = 0M }, "1800");
            }
            else if (product.Position == 3)
            {
                this.ExecuteCommand("COM16", new CoordinateDAO() { X = 430M, Y = 0M, Z = 0M }, "1800");
            }
            else if (product.Position == 4)
            {
                this.ExecuteCommand("COM16", new CoordinateDAO() { X = 241M, Y = 0M, Z = 0M }, "1800");
            }
        }

        public bool SetaFlag()
        {
            return true;
        }

        private void ExecuteCommand(string portName, CoordinateDAO button, String rate)
        {
            var commandGCode = "G90" +
                (button.X != null ? "X" + button.X.ToString().Replace(',', '.') : "") + "" +
                (button.Y != null ? "Y" + button.Y.ToString().Replace(',', '.') : "") + "" +
                (button.Z != null ? "Z" + button.Z.ToString().Replace(',', '.') : "") + "F" + rate;

            this.ExecuteCommand(portName, commandGCode.Trim());

        }
        private void ExecuteCommandRelativo(string portName, CoordinateDAO button, String rate)
        {
            var commandGCode = "G91" +
                (button.X != null ? "X" + button.X.ToString().Replace(',', '.') : "") + "" +
                (button.Y != null ? "Y" + button.Y.ToString().Replace(',', '.') : "") + "" +
                (button.Z != null ? "Z" + button.Z.ToString().Replace(',', '.') : "") + "F" + rate;

            this.ExecuteCommand(portName, commandGCode.Trim());

        }
        private void ExecuteCommand(string portName, string command)
        {
            var port = new SerialPort(portName, 9600, Parity.None, 8, StopBits.One);

            try
            {
                port.Open();
                port.Write(command.Trim() + "\n");
                Thread.Sleep(1000);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                port.Close();
            }
        }
        public string Retorno()
        {
            return "Hello, World!";
        }

        public List<StringResponse> AtualizarProduto(ProductDAO product)
        {
            List<StringResponse> list = new List<StringResponse>();
            var result = ElasticClient.Get<ProductDAO>(product.Id);
            if (result.Found)
            {
                ElasticClient.UpdateAsync<ProductDAO>(product.Id, u => u.Index(result.Index).Doc(new ProductDAO
                {
                    Name = product.Name,
                    Position = product.Position,
                    Coordinate = ProductProvider.Coordinate(product.Position),
                    Validated = product.Validated,
                    ArrivalDateDay = product.ArrivalDateDay,
                    ArrivalDateMonth = product.ArrivalDateMonth,
                    ArrivalDateYear = product.ArrivalDateYear,
                    DeliveryDateDay = product.DeliveryDateDay,
                    DeliveryDateMonth = product.DeliveryDateMonth,
                    DeliveryDateYear = product.DeliveryDateYear,
                    Color = product.Color
                })); ;
                list.Add(new StringResponse { Response = "Product Updated" });
                return list;
            }
            else
            {
                list.Add(new StringResponse { Response = "Product not found" });
                return list;
            }
        }



        public bool MaxQuant()
        {
            if (this.GetAll().Count <= 3) return false;
            else return true;
        }

        public List<BoolResponseDAO> Verificar(string color, string idProduct)
        {
            List<BoolResponseDAO> lista = new List<BoolResponseDAO>();
            var result = ElasticClient.Get<ProductDAO>(idProduct);
            var product = result.Source;
            var comparacao = product.Color == color;
           
                var val = product.Validated;
            ElasticClient.UpdateAsync<ProductDAO>(product.Id, u => u.Index(result.Index).Doc(new ProductDAO {Position = product.Position, Coordinate = ProductProvider.Coordinate(product.Position),
                Validated = comparacao,
                ArrivalDateDay = product.ArrivalDateDay,
                ArrivalDateMonth = product.ArrivalDateMonth,
                ArrivalDateYear = product.ArrivalDateYear,
                DeliveryDateDay = product.DeliveryDateDay,
                DeliveryDateMonth = product.DeliveryDateMonth,
                DeliveryDateYear = product.DeliveryDateYear,
                Color = product.Color,
                Id = product.Id,
                Name = product.Name
            }));
            if (comparacao)
            {
                lista.Add(new BoolResponseDAO { Response = true });
                return lista;
            }
            else  {

                lista.Add(new BoolResponseDAO { Response = false });
                return lista;
            }
        }
        public List<ProductDAO> GetProductById2(string idProduct)
        {
            List<ProductDAO> products = new List<ProductDAO>();
            var result = this.GetProductById(idProduct);
            if(result!=null) products.Add(result);
            return products;
        }
        void PegarPeca(decimal x, decimal y)
        {
            x = x - 70;
            y = y + 41;
            this.ExecuteCommand("COM16", new CoordinateDAO() { X = x, Y = y, Z = 0M }, "1500");
            Thread.Sleep(2000);
            this.ExecuteCommandRelativo("COM16", new CoordinateDAO() { X = 0M, Y = 0M, Z = 127M }, "1500");
            Thread.Sleep(2000);
            this.ExecuteCommandRelativo("COM16", new CoordinateDAO() { X = 0M, Y = 15M, Z = 0M }, "1500");
            Thread.Sleep(2000);
            this.ExecuteCommandRelativo("COM16", new CoordinateDAO() { X = 0M, Y = 0M, Z = -127M }, "1500");
            Thread.Sleep(2000);
            this.ExecuteCommand("COM16", new CoordinateDAO() { X = 3.4M, Y = 57M, Z = 0M }, "1500");
            Thread.Sleep(2000);
            this.ExecuteCommandRelativo("COM16", new CoordinateDAO() { X = 0M, Y = 0M, Z = 127M }, "1500");
            Thread.Sleep(2000);
            this.ExecuteCommand("COM16", new CoordinateDAO() { X = 3.4M, Y = 37M, Z = 127M }, "1500");
            Thread.Sleep(2000);
            this.ExecuteCommandRelativo("COM16", new CoordinateDAO() { X = 0M, Y = 0M, Z = -127M }, "1500");
            Thread.Sleep(2000);
            this.ExecuteCommand("COM16", new CoordinateDAO() { X = 0M, Y = 0M, Z = 0M }, "1500");
        }
        public void Validou(ProductDAO product)
        {
            var result = ElasticClient.Get<ProductDAO>(product.Id);
            if (result.Found)
            {
                if(product.Position == 1)
                {
                    this.PegarPeca(427M, 147M);
                }
                else if(product.Position == 2)
                {
                    this.PegarPeca(251M, 147M);
                }
                else if(product.Position == 3)
                {
                    this.PegarPeca(430M, 0M);
                }
                else if(product.Position == 4)
                {
                    this.PegarPeca(241M, 0M);
                }
            }

        }
    }
}
