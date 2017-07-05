using System;
using System.Reflection;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Xunit;

namespace CSharpViaTest.OtherBCLs.HandleReflections
{
    /* 
     * Description
     * ===========
     * 
     * This test will try get instance member information for a specified type. Please
     * note that the order of report should be in a constructor -> properties -> methods
     * manner. The constructors are ordered by Non-public/public and number of parameters.
     * The properties are ordered by name and the methods are ordered by name and number
     * of parameters.
     * 
     * Difficulty: Super Hard
     * 
     * Knowledge Point
     * ===============
     * 
     * - GetProperties(), GetMethods(), GetConstructors().
     * - BindingFlags enum,
     * - MemberInfo, MethodBase class
     * - Special named methods.
     */
    public class GetMemberInformation
    {
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
        [SuppressMessage("ReSharper", "UnusedParameter.Local")]
        class ForTestCtorProperty
        {
            public ForTestCtorProperty(string name) : this(name, null)
            {
            }

            ForTestCtorProperty(string name, string optional)
            {
                Name = name;
            }

            public string Name { get; }
            public int this[int index] => index;
        }

        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        class ForTestMethod
        {
            public int CalculateSomething(int @base, string name)
            {
                return @base + name.Length;
            }
        }

        #region Please modifies the code to pass the test

        static IEnumerable<string> GetInstanceMemberInformation(Type type)
        {
            var property = type.GetProperties();
            var result =  new List<string> {
                $"Member information for {type.FullName}"
            }.Concat(
                type.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance).Select(c => "Non-public constructor: " + paramterString(c.GetParameters()))
            ).Concat(
                type.GetConstructors(BindingFlags.Public | BindingFlags.Instance).Select(c => "Public constructor: " + paramterString(c.GetParameters()))
            ).Concat(
                type.GetProperties().Where(p => p.GetIndexParameters().Length > 0)
                .Where(p => p.GetGetMethod().IsPublic)
                .Select(p => "Indexed property " + p.Name + ": Public getter.")
            ).Concat(
               type.GetProperties().Where(p => p.GetIndexParameters().Length == 0)
               .Where(p => p.GetGetMethod().IsPublic)
                .Select(p => "Normal property " + p.Name + ": Public getter.")
            ).Concat(
                type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(m => !m.IsSpecialName)
                .Select(m => "Public method " + m.Name + ": " + paramterString(m.GetParameters()))
            ).ToArray();
            return result;
        }

        static string paramterString(ParameterInfo[] parameters) 
        {
            return parameters.Length > 0 ?
                String.Join(", ", parameters.Select(p => p.ParameterType.Name + " " + p.Name)) :
                "no parameter";
        }

        #endregion

        static IEnumerable<object[]> GetMemberTestCases()
        {
            return new[]
            {
                new object[]
                {
                    typeof(ForTestCtorProperty), new[]
                    {
                        "Member information for CSharpViaTest.OtherBCLs.HandleReflections.GetMemberInformation+ForTestCtorProperty",
                        "Non-public constructor: String name, String optional",
                        "Public constructor: String name",
                        "Indexed property Item: Public getter.",
                        "Normal property Name: Public getter."
                    }
                },
                new object[]
                {
                    typeof(ForTestMethod), new []
                    {
                        "Member information for CSharpViaTest.OtherBCLs.HandleReflections.GetMemberInformation+ForTestMethod",
                        "Public constructor: no parameter",
                        "Public method CalculateSomething: Int32 base, String name",
                    }
                }, 
            };
        }

        [Theory]
        [MemberData(nameof(GetMemberTestCases))]
        public void should_get_member_information(Type type, string[] expected)
        {
            Assert.Equal(expected, GetInstanceMemberInformation(type));
        }
    }
}
