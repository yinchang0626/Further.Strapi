using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Volo.Abp.Reflection;

namespace Further.Strapi.Serialization
{
    /// <summary>
    /// Strapi 組件多型類型解析器
    /// 使用 PolymorphicTypeResolver 方式配置組件多型序列化
    /// 利用 ABP 的 ITypeFinder 來獲取已預處理的類型集合，避免重複掃描
    /// </summary>
    public class StrapiPolymorphicTypeResolver : DefaultJsonTypeInfoResolver,Volo.Abp.DependencyInjection.ISingletonDependency
    {
        private readonly ITypeFinder _typeFinder;

        public StrapiPolymorphicTypeResolver(ITypeFinder typeFinder)
        {
            _typeFinder = typeFinder ?? throw new ArgumentNullException(nameof(typeFinder));
        }
        public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
        {
            JsonTypeInfo jsonTypeInfo = base.GetTypeInfo(type, options);

            // 清楚分離父親（基礎介面）和兒子（具體類別）的處理邏輯
            if (IsBaseComponentInterface(type))
            {
                // 父親：處理集合序列化 (List<IStrapiComponent>)
                ConfigureBaseInterfacePolymorphism(jsonTypeInfo);
            }
            else if (IsConcreteComponentClass(type))
            {
                // 兒子：處理單一物件序列化 (TestComponent obj)
                ConfigureConcreteClassPolymorphism(jsonTypeInfo);
            }

            return jsonTypeInfo;
        }

        /// <summary>
        /// 判斷是否為基礎組件介面（父親）
        /// 用於處理集合序列化 List&lt;IStrapiComponent&gt;
        /// </summary>
        private static bool IsBaseComponentInterface(Type type)
        {
            return type == typeof(IStrapiComponent);
        }

        /// <summary>
        /// 判斷是否為具體組件類別（兒子）
        /// 用於處理單一物件序列化 TestComponent obj
        /// 必須有 StrapiComponentName 屬性才能套用多型序列化
        /// </summary>
        private static bool IsConcreteComponentClass(Type type)
        {
            return typeof(IStrapiComponent).IsAssignableFrom(type) &&
                   !type.IsInterface &&
                   !type.IsAbstract &&
                   type != typeof(IStrapiComponent) &&
                   type.GetCustomAttribute<StrapiComponentNameAttribute>() != null;
        }

        /// <summary>
        /// 配置基礎介面多型序列化（父親）
        /// 專門處理集合序列化 List&lt;IStrapiComponent&gt;
        /// 
        /// 在基礎介面上配置所有派生類型，讓 System.Text.Json
        /// 知道如何序列化/反序列化集合中的具體物件
        /// </summary>
        private void ConfigureBaseInterfacePolymorphism(JsonTypeInfo jsonTypeInfo)
        {
            var componentTypes = GetComponentTypes();

            if (componentTypes.Any())
            {
                jsonTypeInfo.PolymorphismOptions = new JsonPolymorphismOptions
                {
                    TypeDiscriminatorPropertyName = "__component",
                    IgnoreUnrecognizedTypeDiscriminators = true,
                    UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization,
                };

                // 為每個組件類型添加 JsonDerivedType
                foreach (var componentType in componentTypes)
                {
                    var componentName = GetComponentName(componentType);
                    if (!string.IsNullOrEmpty(componentName))
                    {
                        jsonTypeInfo.PolymorphismOptions.DerivedTypes.Add(
                            new JsonDerivedType(componentType, componentName));
                    }
                }
            }
        }

        /// <summary>
        /// 配置具體類別多型序列化（兒子）
        /// 專門處理單一物件序列化 TestComponent obj
        /// 
        /// 在具體類型上配置自己的多型選項，確保直接序列化單一物件時
        /// 也能正確包含 __component 識別符
        /// </summary>
        private void ConfigureConcreteClassPolymorphism(JsonTypeInfo jsonTypeInfo)
        {
            var componentName = GetComponentName(jsonTypeInfo.Type);
            
            if (!string.IsNullOrEmpty(componentName))
            {
                jsonTypeInfo.PolymorphismOptions = new JsonPolymorphismOptions
                {
                    TypeDiscriminatorPropertyName = "__component",
                    IgnoreUnrecognizedTypeDiscriminators = true,
                    UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization,
                };

                // 只為當前這個具體類型添加 JsonDerivedType
                jsonTypeInfo.PolymorphismOptions.DerivedTypes.Add(
                    new JsonDerivedType(jsonTypeInfo.Type, componentName));
            }
        }

        /// <summary>
        /// 獲取組件類型列表
        /// 使用 ABP 的 ITypeFinder（已經在 ApplicationInit 階段預處理過）
        /// </summary>
        private List<Type> GetComponentTypes()
        {
            if (_typeFinder == null)
            {
                throw new InvalidOperationException("ITypeFinder is not available. Ensure StrapiSharedModule is properly loaded.");
            }

            // 使用 ABP 的 ITypeFinder（已經在 ApplicationInit 階段預處理過）
            return _typeFinder.Types
                .Where(type => 
                    typeof(IStrapiComponent).IsAssignableFrom(type) &&
                    !type.IsInterface &&
                    !type.IsAbstract &&
                    type != typeof(IStrapiComponent) &&
                    type.GetCustomAttribute<StrapiComponentNameAttribute>() != null)
                .ToList();
        }

        /// <summary>
        /// 取得組件名稱，只使用 ComponentNameAttribute
        /// </summary>
        private static string? GetComponentName(Type componentType)
        {
            // 只接受有 ComponentNameAttribute 的組件
            var attribute = componentType.GetCustomAttribute<StrapiComponentNameAttribute>();
            return attribute?.ComponentName;
        }
    }
}