using ClassesDAO.DAO;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TCC___Implementation.Interface
{
    public interface ICommunication
    {
        string Retorno();
        List<ProductDAO> GetAll();
        ProductDAO GetProductById(string id);
        bool IndexExists(string index);
        void CheckIndex(Object obj, string indexname);
        void IndexDocument(Object obj, out string erro);
        bool DocumentExists(string id);
        List<BoolResponseDAO> DeleteById(string id, out string erro);
        void Validou(ProductDAO product);
        bool SetaFlag();
        void Inicializacao();
        void Validate(string idProduct);
        List<ProductDAO> GetProductById2(string idProduct);
        List<BoolResponseDAO> Verificar(string color, string idProduct);
        bool MaxQuant();
        List<StringResponse> AtualizarProduto(ProductDAO product);
    }
}
