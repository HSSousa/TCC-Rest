using ClassesDAO.DAO;
using ElasticSearch.Service;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using TCC___Implementation.Interface;
using TCC___Implementation.Provider;

namespace TCC___Rest.Controllers
{
    [Route("product")]
    [ApiController]
    public class ProductController : ControllerBase
    {

        public static ICommunication service = new ElasticSearchService<ProductDAO>("product");
        public static string ProdutoID;
        public static bool flag;

        [HttpGet("call")]
        public ActionResult Call1()
        {
            return Ok(service.Retorno());
        }
        [HttpGet("{color}/verificar")]
        public  ActionResult Verificar(string color)
        {
            if (flag)
            {
                var result = service.Verificar(color, ProdutoID);
                flag = false;
                return Accepted(result);
            }
            else return BadRequest();
        }

        [HttpGet("{idProduct}")]
        public ActionResult GetProductById(string idProduct)
        {
            var result = service.GetProductById2(idProduct);
            return Accepted(result);
        }

        [HttpPost("validou")]
        public ActionResult Validou(ProductDAO product)
        {
            service.Validou(product);
            return Ok();
        }
        [HttpPost("home")]
        public ActionResult Home()
        {
            service.Inicializacao();
            return Ok();
        }

        [HttpPost("{id}/pegar")]
        public ActionResult PegarProduto(string id)
        {
            if (id != null)
            {
                ProdutoID = id;
                service.Validate(id);
                return Ok();
            }
            else return BadRequest("PRODUCTID_IS_NULL");
        }
        [HttpPost("{id}/add")]
        public ActionResult<string> AdicionarProduto(string id, ProductDAO productDAO)
        {
                productDAO.Id = id;
                productDAO.Coordinate = ProductProvider.Coordinate(productDAO.Position);
            string erro = string.Empty;
            if (!service.MaxQuant()) {
                service.IndexDocument(productDAO, out erro);
                if(erro == string.Empty){
                    return Ok("The product has been added");
                }
                else return BadRequest(erro);
            }
            else return BadRequest("Maximum product limit reached");
        }
        [HttpPost("setaflag")]
        public ActionResult SetaFlag()
        {
            flag = service.SetaFlag();
            return Ok();
        }

        [HttpPut()]
        public ActionResult AtualizarProduto(ProductDAO product)
        {
            return Ok(service.AtualizarProduto(product));
        }
      
        

        [HttpGet("getAll")]
        public ActionResult GetAll()
        {

            return Ok(service.GetAll());
        }
        [HttpDelete("{produtoID}")]
        public ActionResult RemoveProductById(string produtoID)
        {
            string erro = string.Empty;
            var response = service.DeleteById(produtoID,out erro);
            if(erro == string.Empty)
            {
                return Accepted(response);
            }
            Console.WriteLine(erro);
            return BadRequest(response);
        }
    }
}
