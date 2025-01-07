using System.Linq;
using System.Web.Mvc;
using Unity.AspNet.Mvc;

[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(HeartSpace.App_Start.UnityMvcActivator), "Start")]
[assembly: WebActivatorEx.ApplicationShutdownMethod(typeof(HeartSpace.App_Start.UnityMvcActivator), "Shutdown")]

namespace HeartSpace.App_Start
{
    public static class UnityMvcActivator
    {
        /// <summary>
        /// 在應用程式啟動時整合 Unity。
        /// </summary>
        public static void Start()
        {
            if (UnityConfig.Container == null)
            {
                UnityConfig.RegisterComponents(); // 如果容器尚未初始化，則手動初始化
            }

            FilterProviders.Providers.Remove(FilterProviders.Providers.OfType<FilterAttributeFilterProvider>().First());
            FilterProviders.Providers.Add(new UnityFilterAttributeFilterProvider(UnityConfig.Container));

            DependencyResolver.SetResolver(new UnityDependencyResolver(UnityConfig.Container));
        }
        

        /// <summary>
        /// 在應用程式關閉時釋放 Unity 容器。
        /// </summary>
        public static void Shutdown()
        {
            UnityConfig.Container.Dispose();
        }
    }
}
