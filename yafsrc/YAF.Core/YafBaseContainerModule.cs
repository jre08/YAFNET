/* Yet Another Forum.NET
 * Copyright (C) 2006-2010 Jaben Cargman
 * http://www.yetanotherforum.net/
 * 
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
 */
namespace YAF.Core
{
  #region Using

  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Reflection;
  using System.Web;

  using Autofac;
  using Autofac.Core;

  using Moq;

  using YAF.Classes;
  using YAF.Core.Services;
  using YAF.Types;
  using YAF.Types.Attributes;
  using YAF.Types.Interfaces;
  using YAF.Utils;

  using Module = Autofac.Module;

  #endregion

  /// <summary>
  /// The module for all singleton scoped items...
  /// </summary>
  public class YafBaseContainerModule : Module
  {
    #region Methods

    /// <summary>
    /// The load.
    /// </summary>
    /// <param name="builder">
    /// The builder.
    /// </param>
    protected override void Load([NotNull] ContainerBuilder builder)
    {
      this.RegisterBasicBindings(builder);

      this.RegisterEventBindings(builder);

      this.RegisterWebAbstractions(builder);

      this.RegisterServices(builder);

      this.RegisterModules(builder);

      this.RegisterExternalModules(builder);
    }

    /// <summary>
    /// The get assembly sort order.
    /// </summary>
    /// <param name="a">
    /// The a.
    /// </param>
    /// <returns>
    /// The get assembly sort order.
    /// </returns>
    private int GetAssemblySortOrder([NotNull] Assembly a)
    {
      var attribute = a.GetCustomAttributes(typeof(AssemblyModuleSortOrder), true).OfType<AssemblyModuleSortOrder>();

      return attribute.Any() ? attribute.First().SortOrder : 9999;
    }

    /// <summary>
    /// The register basic bindings.
    /// </summary>
    /// <param name="builder">
    /// The builder.
    /// </param>
    private void RegisterBasicBindings([NotNull] ContainerBuilder builder)
    {
      CodeContracts.ArgumentNotNull(builder, "builder");

      builder.RegisterType<AutoFacServiceLocatorProvider>().AsSelf().As<IServiceLocator>().OwnedByLifetimeScope();

      // the scopes will probably change
      builder.RegisterType<YafSendMail>().As<ISendMail>().OwnedByLifetimeScope();
      builder.RegisterType<YafSession>().As<IYafSession>().OwnedByLifetimeScope();
      builder.RegisterType<YafDBBroker>().As<IDBBroker>().OwnedByLifetimeScope();
      builder.RegisterType<YafBadWordReplace>().As<IBadWordReplace>().OwnedByLifetimeScope();
      builder.RegisterType<YafPermissions>().As<IPermissions>().OwnedByLifetimeScope();
      builder.RegisterType<YafDateTime>().As<IDateTime>().OwnedByLifetimeScope();
      builder.RegisterType<YafAvatars>().As<IAvatars>().OwnedByLifetimeScope();
      builder.RegisterType<YafFavoriteTopic>().As<IFavoriteTopic>().OwnedByLifetimeScope();
      builder.RegisterType<YafUserIgnored>().As<IUserIgnored>().OwnedByLifetimeScope();
      builder.RegisterType<YafSendNotification>().As<ISendNotification>().OwnedByLifetimeScope();
      builder.RegisterType<YafDigest>().As<IDigest>().OwnedByLifetimeScope();

      builder.RegisterType<DefaultUserDisplayName>().As<IUserDisplayName>().OwnedByLifetimeScope();

      builder.RegisterType<DefaultUrlBuilder>().As<IUrlBuilder>().OwnedByLifetimeScope();

      builder.RegisterType<RewriteUrlBuilder>().Named<IUrlBuilder>("rewriter").OwnedByLifetimeScope();

      builder.RegisterType<YafStopWatch>().As<IStopWatch>().InstancePerLifetimeScope();

      // localization registration...
      builder.RegisterType<LocalizationProvider>().InstancePerLifetimeScope();
      builder.Register(k => k.Resolve<LocalizationProvider>().Localization);

      // theme registration...
      builder.RegisterType<ThemeProvider>().InstancePerLifetimeScope();
      builder.Register(k => k.Resolve<ThemeProvider>().Theme);

      // module resolution bindings...
      builder.RegisterGeneric(typeof(StandardModuleManager<>)).As(typeof(IModuleManager<>)).InstancePerLifetimeScope();
    }

    /// <summary>
    /// Register event bindings
    /// </summary>
    /// <param name="builder">
    /// </param>
    private void RegisterEventBindings([NotNull] ContainerBuilder builder)
    {
      builder.RegisterType<ServiceLocatorEventRaiser>().As<IRaiseEvent>().SingleInstance();
      builder.RegisterGeneric(typeof(FireEvent<>)).As(typeof(IFireEvent<>)).InstancePerLifetimeScope();

      // scan assemblies for events to wire up...
      var assemblies =
        AppDomain.CurrentDomain.GetAssemblies().Where(a => a.FullName.IsSet() && a.FullName.ToLower().StartsWith("yaf"))
          .ToArray();

      builder.RegisterAssemblyTypes(assemblies).AsClosedTypesOf(typeof(IHandleEvent<>)).AsImplementedInterfaces().
        InstancePerLifetimeScope();
    }

    /// <summary>
    /// The register external modules.
    /// </summary>
    /// <param name="builder">
    /// The builder.
    /// </param>
    private void RegisterExternalModules([NotNull] ContainerBuilder builder)
    {
      CodeContracts.ArgumentNotNull(builder, "builder");

      var moduleList =
        AppDomain.CurrentDomain.GetAssemblies().Where(a => a.FullName.IsSet() && a.FullName.ToLower().StartsWith("yaf"))
          .ToList();

      // make sure we don't include this assembly -- otherwise we'll have a recusive situation.
      moduleList.Remove(Assembly.GetExecutingAssembly());

      // little bit of filtering...
      moduleList.OrderByDescending(this.GetAssemblySortOrder);

      // TODO: create real abstracted plugin model. This is a stop-gap.
      var modules = moduleList.FindModules<IModule>();

      foreach (var moduleInstance in modules)
      {
        builder.RegisterModule(Activator.CreateInstance(moduleInstance) as IModule);
      }
    }

    /// <summary>
    /// The register modules.
    /// </summary>
    /// <param name="builder">
    /// The builder.
    /// </param>
    private void RegisterModules([NotNull] ContainerBuilder builder)
    {
      CodeContracts.ArgumentNotNull(builder, "builder");

      var assemblies =
        AppDomain.CurrentDomain.GetAssemblies().Where(a => a.FullName.IsSet() && a.FullName.ToLower().StartsWith("yaf"))
          .ToArray();

      // forum modules...
      builder.RegisterAssemblyTypes(assemblies).AssignableTo<IBaseForumModule>().As<IBaseForumModule>().
        InstancePerLifetimeScope();

      // editor modules...
      builder.RegisterAssemblyTypes(assemblies).AssignableTo<ForumEditor>().As<ForumEditor>().InstancePerLifetimeScope();
    }

    /// <summary>
    /// The register services.
    /// </summary>
    /// <param name="builder">
    /// The builder.
    /// </param>
    private void RegisterServices([NotNull] ContainerBuilder builder)
    {
      CodeContracts.ArgumentNotNull(builder, "builder");

      builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly()).AssignableTo<IStartupService>().As<IStartupService>
        ().InstancePerLifetimeScope();

      builder.Register(
        x =>
        x.Resolve<IEnumerable<IStartupService>>().Where(t => t is StartupInitializeDb).FirstOrDefault() as
        StartupInitializeDb).InstancePerLifetimeScope();
    }

    /// <summary>
    /// The register web abstractions.
    /// </summary>
    /// <param name="builder">
    /// The builder.
    /// </param>
    private void RegisterWebAbstractions([NotNull] ContainerBuilder builder)
    {
      CodeContracts.ArgumentNotNull(builder, "builder");

      builder.Register(
        k =>
        HttpContext.Current != null ? new HttpContextWrapper(HttpContext.Current) : new Mock<HttpContextBase>().Object).
        InstancePerLifetimeScope();

      builder.Register(
        k =>
        HttpContext.Current != null
          ? new HttpSessionStateWrapper(HttpContext.Current.Session)
          : new Mock<HttpSessionStateBase>().Object).InstancePerLifetimeScope();

      builder.Register(
        k =>
        HttpContext.Current != null
          ? new HttpRequestWrapper(HttpContext.Current.Request)
          : new Mock<HttpRequestBase>().Object).InstancePerLifetimeScope();

      builder.Register(
        k =>
        HttpContext.Current != null
          ? new HttpResponseWrapper(HttpContext.Current.Response)
          : new Mock<HttpResponseBase>().Object).InstancePerLifetimeScope();

      builder.Register(
        k =>
        HttpContext.Current != null
          ? new HttpServerUtilityWrapper(HttpContext.Current.Server)
          : new Mock<HttpServerUtilityBase>().Object).InstancePerLifetimeScope();

      builder.Register(
        k =>
        HttpContext.Current != null
          ? new HttpApplicationStateWrapper(HttpContext.Current.Application)
          : new Mock<HttpApplicationStateBase>().Object).InstancePerLifetimeScope();
    }

    #endregion
  }
}