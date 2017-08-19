﻿using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;

namespace NetInject.Cecil
{
    public static class CecilHelper
    {
        public static IEnumerable<TypeDefinition> GetAllTypes(this AssemblyDefinition ass)
            => ass.Modules.SelectMany(m => m.GetAllTypes());

        public static IEnumerable<TypeDefinition> GetAllTypes(this ModuleDefinition mod)
            => mod.Types.SelectMany(t => t.GetAllTypes());

        public static IEnumerable<TypeDefinition> GetAllTypes(this TypeDefinition type)
            => (new[] {type}).Concat(type.NestedTypes.SelectMany(t => t.GetAllTypes()));

        public static IEnumerable<TypeReference> GetAllTypeRefs(this AssemblyDefinition ass)
            => ass.Modules.SelectMany(m => m.GetTypeReferences());

        public static IEnumerable<MemberReference> GetAllMemberRefs(this AssemblyDefinition ass)
            => ass.Modules.SelectMany(m => m.GetMemberReferences());

        public static bool IsStandardLib(string key)
            => key == "mscorlib" || key == "System" ||
               key == "System.Core" || key == "Microsoft.CSharp";

        public static bool IsDelegate(this TypeDefinition type)
            => type?.BaseType?.FullName == typeof(System.MulticastDelegate).FullName
               || type?.BaseType?.FullName == typeof(System.Delegate).FullName;

        public static string GetParamStr(IMetadataTokenProvider meth)
            => meth.ToString().Split(new[] {'('}, 2).Last().TrimEnd(')');

        public static bool IsInStandardLib(this TypeReference type)
        {
            var assRef = type.Scope as AssemblyNameReference;
            var modRef = type.Scope as ModuleDefinition;
            return (assRef == null || IsStandardLib(assRef.Name)) && modRef == null;
        }

        public static bool ContainsType(AssemblyNameReference assRef, TypeReference typRef)
            => assRef.FullName == (typRef.Scope as AssemblyNameReference)?.FullName;

        public static bool ContainsMember(AssemblyNameReference assRef, MemberReference mbmRef)
            => ContainsType(assRef, mbmRef.DeclaringType);

        public static TypeKind GetTypeKind(this TypeDefinition typeDef)
        {
            if (typeDef.IsEnum)
                return TypeKind.Enum;
            if (typeDef.IsValueType)
                return TypeKind.Struct;
            if (typeDef.IsDelegate())
                return TypeKind.Delegate;
            if (typeDef.IsInterface)
                return TypeKind.Interface;
            if (typeDef.IsClass)
                return TypeKind.Class;
            return default(TypeKind);
        }

        public static bool IsBaseCandidate(this TypeDefinition myTypeDef)
            => !myTypeDef.IsValueType && !myTypeDef.IsSpecialName && !myTypeDef.IsSealed
               && !myTypeDef.IsRuntimeSpecialName && !myTypeDef.IsPrimitive
               && !myTypeDef.IsInterface && !myTypeDef.IsArray &&
               (myTypeDef.IsPublic || myTypeDef.IsNestedPublic) &&
               myTypeDef.IsClass;

        public static IEnumerable<TypeDefinition> GetDerivedTypes(this AssemblyDefinition ass,
            TypeReference baseType)
            => ass.GetAllTypes().Where(type => type.BaseType == baseType);

        public static IEnumerable<MemberReference> GetAllMembers(this TypeDefinition typeDef)
        {
            var evts = typeDef.Events.Cast<MemberReference>();
            var filds = typeDef.Fields.Cast<MemberReference>();
            var meths = typeDef.Methods.Cast<MemberReference>();
            var props = typeDef.Properties.Cast<MemberReference>();
            return evts.Concat(filds).Concat(meths).Concat(props).Distinct();
        }
    }
}