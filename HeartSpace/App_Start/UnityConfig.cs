using HeartSpace.Models.Repositories;
using HeartSpace.Models.Services;
using System.Web.Mvc;
using Unity;
using Unity.Mvc5;

namespace HeartSpace.App_Start
{
    public static class UnityConfig
    {
        // 靜態容器
        public static IUnityContainer Container { get; private set; }

        public static void RegisterComponents()
        {
            // 初始化容器
            Container = new UnityContainer();

            // 註冊 Repository 和 Service
            Container.RegisterType<PostEFRepository>();
            Container.RegisterType<PostService>();

            // 設定 MVC 的解析器
            DependencyResolver.SetResolver(new UnityDependencyResolver(Container));
        }
    }
}
