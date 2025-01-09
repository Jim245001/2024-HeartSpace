using HeartSpace.DTOs.Services.Interfaces;
using HeartSpace.Models.EFModels;
using HeartSpace.Models.Repositories;
using HeartSpace.Models.Services;
using System.ComponentModel;
using System.Web.Mvc;
using Unity;
using Unity.Injection;
using Unity.Mvc5;

namespace HeartSpace.App_Start
{
    public static class UnityConfig
    {
        // 靜態容器
        public static IUnityContainer Container { get; private set; }

        public static void RegisterComponents()
        {
            // 註冊 AppDbContext
            //Container.RegisterType<AppDbContext>(new InjectionConstructor("name=AppDbContext"));

            // 初始化容器
            Container = new UnityContainer();

            // 註冊 Repository 和 Service
            Container.RegisterType<IPostService, PostService>();

            // 設置 DependencyResolver
            System.Web.Mvc.DependencyResolver.SetResolver(new UnityDependencyResolver(Container));
        }
    }
}
