using FluentValidation;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MediatR;
using OrderManagementSystem.Shared.Enums;
using OrderManagementSystem.Shared.Exceptions;
using OrderService.Api.Protos;
using OrderService.Application.Commands.UpdateOrderStatusCommand;

namespace OrderService.Api.GrpcServices
{
    public class OrderGrpcService(IMediator mediator) : Order.OrderBase
    {
        public override async Task<Empty> UpdateOrderStatus(UpdateOrderStatusRequest request, ServerCallContext context)
        {
            if (!System.Enum.TryParse<OrderStatus>(request.NewStatus, out var status))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid order status."));
            }
            try
            {
                var cmd = new UpdateOrderStatusCommand(
                    Guid.Parse(request.OrderId),
                    status);

                await mediator.Send(cmd, context.CancellationToken);
                return new Empty();
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
    }
}
