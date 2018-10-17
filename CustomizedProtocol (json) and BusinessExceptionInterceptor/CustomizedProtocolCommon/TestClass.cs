using AspectCore.Extensions.Reflection;
using DotNetty.Buffers;
using Newtonsoft.Json;
using System;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Tars.Net.Codecs;
using Tars.Net.Metadata;

namespace CustomizedProtocolCommon
{
    public class TestDecoder : IDecoder<IByteBuffer>
    {
        public TestDecoder(IRpcMetadata rpcMetadata)
        {
            this.rpcMetadata = rpcMetadata;
        }

        private static readonly MethodInfo FromResult = typeof(Task).GetTypeInfo().GetMethod(nameof(Task.FromResult));
        private readonly IRpcMetadata rpcMetadata;

        public Request DecodeRequest(IByteBuffer input)
        {
            var req =
                    JsonConvert.DeserializeObject<Request>(input.ReadString(input.ReadableBytes, Encoding.UTF8));
            input.MarkReaderIndex();
            var (method, isOneway, outParameters, codec, version, serviceType) = rpcMetadata.FindRpcMethod(req.ServantName, req.FuncName);
            req.IsOneway = isOneway;
            req.Mehtod = method;
            req.ReturnParameterTypes = outParameters;
            req.ServiceType = serviceType;
            req.ParameterTypes = method.GetParameters();
            for (int i = 0; i < req.ParameterTypes.Length; i++)
            {
                if (req.Parameters[i] == null || !req.ParameterTypes[i].ParameterType.IsValueType)
                {
                    continue;
                }
                req.Parameters[i] = Convert.ChangeType(req.Parameters[i], req.ParameterTypes[i].ParameterType);
            }
            return req;
        }

        public Response DecodeResponse(IByteBuffer input)
        {
            var resp = JsonConvert.DeserializeObject<Response>(input.ReadString(input.ReadableBytes, Encoding.UTF8));
            input.MarkReaderIndex();
            var (method, isOneway, outParameters, codec, version, serviceType) = rpcMetadata.FindRpcMethod(resp.ServantName, resp.FuncName);
            resp.ReturnValueType = method.ReturnParameter;
            resp.ReturnParameterTypes = outParameters;
            resp.ReturnParameters = new object[outParameters.Length];
            for (int i = 0; i < resp.ReturnParameterTypes.Length; i++)
            {
                if (resp.ReturnParameterTypes[i] == null || !resp.ReturnParameterTypes[i].ParameterType.IsValueType)
                {
                    continue;
                }
                resp.ReturnParameters[i] = Convert.ChangeType(resp.ReturnParameters[i], resp.ReturnParameterTypes[i].ParameterType);
            }
            var type = resp.ReturnValueType.ParameterType;
            var info = type.GetTypeInfo();
            if (info.IsTask())
            {
                resp.ReturnValue = Task.CompletedTask;
            }
            else if (info.IsTaskWithResult())
            {
                resp.ReturnValue = FromResult.MakeGenericMethod(type.GetGenericArguments()).GetReflector().StaticInvoke(resp.ReturnValue);
            }
            else if (info.IsValueTask())
            {
                resp.ReturnValue = Activator.CreateInstance(type, resp.ReturnValue);
            }
            else if (resp.ReturnValue != null || !type.IsValueType)
            {
                resp.ReturnValue = Convert.ChangeType(resp.ReturnValue, resp.ReturnValueType.ParameterType);
            }
            return resp;
        }
    }

    public class TestEncoder : IEncoder<IByteBuffer>
    {
        public IByteBuffer EncodeRequest(Request req)
        {
            req.ParameterTypes = null;
            return Unpooled.WrappedBuffer(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(req)));
        }

        public IByteBuffer EncodeResponse(Response message)
        {
            if (message.ReturnValueType != null)
            {
                var type = message.ReturnValueType.ParameterType;
                var info = type.GetTypeInfo();
                if (info.IsTask() || type == typeof(void))
                {
                    message.ReturnValue = null;
                }
                else if (message.ReturnValue != null && (info.IsValueTask() || info.IsTaskWithResult()))
                {
                    message.ReturnValue = type.GetProperty("Result").GetReflector().GetValue(message.ReturnValue);
                }
                message.ReturnParameterTypes = null;
                message.ReturnValueType = null;
            }
            return Unpooled.WrappedBuffer(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message)));
        }
    }
}