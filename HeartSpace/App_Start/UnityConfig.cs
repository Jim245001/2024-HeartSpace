//using HeartSpace.Models.Repositories;
//using HeartSpace.Models.Services;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web;
//using System.Web.Mvc;
//using Unity;
//using Unity.Mvc5;



//namespace HeartSpace.App_Start
//{
//    public class UnityConfig
//    {
//        public static void RegisterComponents()
//        {
//            var container = new UnityContainer();

//            // 註冊 Repository 和 Service
//            container.RegisterType<PostEFRepository>();
//            container.RegisterType<PostService>();

//            // 註冊 Controller 的依賴
//            DependencyResolver.SetResolver(new UnityDependencyResolver(container));
//        }
//    }
//}