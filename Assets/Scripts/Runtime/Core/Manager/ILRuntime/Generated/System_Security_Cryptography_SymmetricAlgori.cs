using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

using ILRuntime.CLR.TypeSystem;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;
using ILRuntime.Runtime.Stack;
using ILRuntime.Reflection;
using ILRuntime.CLR.Utils;

namespace ILRuntime.Runtime.Generated
{
    unsafe class System_Security_Cryptography_SymmetricAlgorithm_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodBase method;
            Type[] args;
            Type type = typeof(System.Security.Cryptography.SymmetricAlgorithm);
            args = new Type[]{typeof(System.Byte[])};
            method = type.GetMethod("set_Key", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, set_Key_0);
            args = new Type[]{typeof(System.Security.Cryptography.CipherMode)};
            method = type.GetMethod("set_Mode", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, set_Mode_1);
            args = new Type[]{typeof(System.Security.Cryptography.PaddingMode)};
            method = type.GetMethod("set_Padding", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, set_Padding_2);
            args = new Type[]{};
            method = type.GetMethod("CreateEncryptor", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, CreateEncryptor_3);
            args = new Type[]{};
            method = type.GetMethod("CreateDecryptor", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, CreateDecryptor_4);


        }


        static StackObject* set_Key_0(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Byte[] @value = (System.Byte[])typeof(System.Byte[]).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Security.Cryptography.SymmetricAlgorithm instance_of_this_method = (System.Security.Cryptography.SymmetricAlgorithm)typeof(System.Security.Cryptography.SymmetricAlgorithm).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.Key = value;

            return __ret;
        }

        static StackObject* set_Mode_1(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Security.Cryptography.CipherMode @value = (System.Security.Cryptography.CipherMode)typeof(System.Security.Cryptography.CipherMode).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Security.Cryptography.SymmetricAlgorithm instance_of_this_method = (System.Security.Cryptography.SymmetricAlgorithm)typeof(System.Security.Cryptography.SymmetricAlgorithm).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.Mode = value;

            return __ret;
        }

        static StackObject* set_Padding_2(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Security.Cryptography.PaddingMode @value = (System.Security.Cryptography.PaddingMode)typeof(System.Security.Cryptography.PaddingMode).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Security.Cryptography.SymmetricAlgorithm instance_of_this_method = (System.Security.Cryptography.SymmetricAlgorithm)typeof(System.Security.Cryptography.SymmetricAlgorithm).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.Padding = value;

            return __ret;
        }

        static StackObject* CreateEncryptor_3(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Security.Cryptography.SymmetricAlgorithm instance_of_this_method = (System.Security.Cryptography.SymmetricAlgorithm)typeof(System.Security.Cryptography.SymmetricAlgorithm).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = instance_of_this_method.CreateEncryptor();

            object obj_result_of_this_method = result_of_this_method;
            if(obj_result_of_this_method is CrossBindingAdaptorType)
            {    
                return ILIntepreter.PushObject(__ret, __mStack, ((CrossBindingAdaptorType)obj_result_of_this_method).ILInstance);
            }
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static StackObject* CreateDecryptor_4(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Security.Cryptography.SymmetricAlgorithm instance_of_this_method = (System.Security.Cryptography.SymmetricAlgorithm)typeof(System.Security.Cryptography.SymmetricAlgorithm).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = instance_of_this_method.CreateDecryptor();

            object obj_result_of_this_method = result_of_this_method;
            if(obj_result_of_this_method is CrossBindingAdaptorType)
            {    
                return ILIntepreter.PushObject(__ret, __mStack, ((CrossBindingAdaptorType)obj_result_of_this_method).ILInstance);
            }
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }



    }
}
