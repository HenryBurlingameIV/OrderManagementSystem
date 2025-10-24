using Azure.Core;
using CatalogService.Api.Protos;
using CatalogService.Application.Contracts;
using CatalogService.Application.Services;
using FluentValidation;
using Grpc.Core;
using OrderManagementSystem.Shared.Exceptions;

namespace CatalogService.Api.GrpcServices
{
    public class CatalogGrpcService(IProductService productService) : Catalog.CatalogBase
    {
        public override async Task<ProductResponse> ReserveProduct(ReserveProductRequest request, ServerCallContext context)
        {
            try
            {
                var result = await productService.ReserveProductAsync(
                    Guid.Parse(request.ProductId),
                    request.Quantity,
                    context.CancellationToken);

                return new ProductResponse()
                {
                    ProductId = result.Id.ToString(),
                    Price = result.Price.ToString("F2"),
                };
            }
            catch (NotFoundException ex)
            {
                throw new RpcException(new Status(StatusCode.NotFound, ex.Message));
            }
            catch (ValidationException ex)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
            }
            catch (Exception ex)
            {
                throw new RpcException(new Status(StatusCode.Internal, "Internal server error"));
            }
        }

        public override async Task<ProductResponse> ReleaseProduct(ReleaseProductRequest request, ServerCallContext context)
        {
            try
            {
                var result = await productService.ReleaseProductAsync(
                Guid.Parse(request.ProductId),
                request.Quantity,
                context.CancellationToken
                );

                return new ProductResponse()
                {
                    ProductId = result.Id.ToString(),
                    Price = result.Price.ToString("F2"),
                };

            }
            catch (NotFoundException ex)
            {
                throw new RpcException(new Status(StatusCode.NotFound, ex.Message));
            }
            catch (ValidationException ex)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("not have enough quantity"))
            {
                throw new RpcException(new Status(StatusCode.FailedPrecondition, ex.Message));
            }
            catch (Exception ex)
            {
                throw new RpcException(new Status(StatusCode.Internal, "Internal server error"));
            }

        }
    }
}
