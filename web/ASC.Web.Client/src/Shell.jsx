import React, { useEffect } from "react";
import { Router, Switch, Route } from "react-router-dom";
import { inject, observer } from "mobx-react";
import NavMenu from "./components/NavMenu";
import Main from "./components/Main";
import PrivateRoute from "@appserver/common/components/PrivateRoute";
import PublicRoute from "@appserver/common/components/PublicRoute";
import ErrorBoundary from "@appserver/common/components/ErrorBoundary";
import Layout from "./components/Layout";
import ScrollToTop from "./components/Layout/ScrollToTop";
import history from "@appserver/common/history";
import toastr from "studio/toastr";
import { combineUrl, updateTempContent } from "@appserver/common/utils";
import { Provider as MobxProvider } from "mobx-react";
import ThemeProvider from "@appserver/components/theme-provider";
import { Base } from "@appserver/components/themes";
import store from "studio/store";
import config from "../package.json";
import { I18nextProvider } from "react-i18next";
import i18n from "./i18n";
import AppLoader from "@appserver/common/components/AppLoader";
import System from "./components/System";
import { AppServerConfig } from "@appserver/common/constants";

const { proxyURL } = AppServerConfig;
const homepage = config.homepage;

const PROXY_HOMEPAGE_URL = combineUrl(proxyURL, homepage);
const HOME_URLS = [
  combineUrl(PROXY_HOMEPAGE_URL),
  combineUrl(PROXY_HOMEPAGE_URL, "/"),
  combineUrl(PROXY_HOMEPAGE_URL, "/error=:error"),
];
const WIZARD_URL = combineUrl(PROXY_HOMEPAGE_URL, "/wizard");
const ABOUT_URL = combineUrl(PROXY_HOMEPAGE_URL, "/about");
const LOGIN_URLS = [
  combineUrl(PROXY_HOMEPAGE_URL, "/login"),
  combineUrl(PROXY_HOMEPAGE_URL, "/login/error=:error"),
  combineUrl(PROXY_HOMEPAGE_URL, "/login/confirmed-email=:confirmedEmail"),
];
const CONFIRM_URL = combineUrl(PROXY_HOMEPAGE_URL, "/confirm");
const COMING_SOON_URLS = [combineUrl(PROXY_HOMEPAGE_URL, "/coming-soon")];
const PAYMENTS_URL = combineUrl(PROXY_HOMEPAGE_URL, "/payments");
const SETTINGS_URL = combineUrl(PROXY_HOMEPAGE_URL, "/settings");
const ERROR_401_URL = combineUrl(PROXY_HOMEPAGE_URL, "/error401");
const PROFILE_MY_URL = combineUrl(PROXY_HOMEPAGE_URL, "/my");

const Payments = React.lazy(() => import("./components/pages/Payments"));
const Error404 = React.lazy(() => import("studio/Error404"));
const Error401 = React.lazy(() => import("studio/Error401"));
const Home = React.lazy(() => import("./components/pages/Home"));
const Login = React.lazy(() => import("login/app"));
const About = React.lazy(() => import("./components/pages/About"));
const Wizard = React.lazy(() => import("./components/pages/Wizard"));
const Settings = React.lazy(() => import("./components/pages/Settings"));
const ComingSoon = React.lazy(() => import("./components/pages/ComingSoon"));
const Confirm = React.lazy(() => import("./components/pages/Confirm"));
const MyProfile = React.lazy(() => import("people/MyProfile"));

const SettingsRoute = (props) => (
  <React.Suspense fallback={<AppLoader />}>
    <ErrorBoundary>
      <Settings {...props} />
    </ErrorBoundary>
  </React.Suspense>
);
const PaymentsRoute = (props) => (
  <React.Suspense fallback={<AppLoader />}>
    <ErrorBoundary>
      <Payments {...props} />
    </ErrorBoundary>
  </React.Suspense>
);

const Error404Route = (props) => (
  <React.Suspense fallback={<AppLoader />}>
    <ErrorBoundary>
      <Error404 {...props} />
    </ErrorBoundary>
  </React.Suspense>
);

const Error401Route = (props) => (
  <React.Suspense fallback={<AppLoader />}>
    <ErrorBoundary>
      <Error401 {...props} />
    </ErrorBoundary>
  </React.Suspense>
);
const HomeRoute = (props) => (
  <React.Suspense fallback={<AppLoader />}>
    <ErrorBoundary>
      <Home {...props} />
    </ErrorBoundary>
  </React.Suspense>
);

const ConfirmRoute = (props) => (
  <React.Suspense fallback={<AppLoader />}>
    <ErrorBoundary>
      <Confirm {...props} />
    </ErrorBoundary>
  </React.Suspense>
);

const LoginRoute = (props) => (
  <React.Suspense fallback={<AppLoader />}>
    <ErrorBoundary>
      <Login {...props} />
    </ErrorBoundary>
  </React.Suspense>
);

const AboutRoute = (props) => (
  <React.Suspense fallback={<AppLoader />}>
    <ErrorBoundary>
      <About {...props} />
    </ErrorBoundary>
  </React.Suspense>
);

const WizardRoute = (props) => (
  <React.Suspense fallback={<AppLoader />}>
    <ErrorBoundary>
      <Wizard {...props} />
    </ErrorBoundary>
  </React.Suspense>
);

const ComingSoonRoute = (props) => (
  <React.Suspense fallback={<AppLoader />}>
    <ErrorBoundary>
      <ComingSoon {...props} />
    </ErrorBoundary>
  </React.Suspense>
);

const MyProfileRoute = (props) => (
  <React.Suspense fallback={<AppLoader />}>
    <ErrorBoundary>
      <MyProfile {...props} />
    </ErrorBoundary>
  </React.Suspense>
);

const Shell = ({ items = [], page = "home", ...rest }) => {
  const { isLoaded, loadBaseInfo, modules, isDesktop } = rest;

  useEffect(() => {
    try {
      if (!window.AppServer) {
        window.AppServer = {};
      }

      //TEMP object, will be removed!!!
      window.AppServer.studio = {
        HOME_URLS,
        WIZARD_URL,
        ABOUT_URL,
        LOGIN_URLS,
        CONFIRM_URL,
        COMING_SOON_URLS,
        PAYMENTS_URL,
        SETTINGS_URL,
        ERROR_401_URL,
      };

      loadBaseInfo();
    } catch (err) {
      toastr.error(err);
    }
  }, []);

  useEffect(() => {
    if (isLoaded) updateTempContent();
  }, [isLoaded]);

  useEffect(() => {
    console.log("Current page ", page);
  }, [page]);

  const pathname = window.location.pathname.toLowerCase();
  const isEditor = pathname.indexOf("doceditor") !== -1;

  if (!window.AppServer.studio) {
    window.AppServer.studio = {};
  }

  window.AppServer.studio.modules = {};

  const dynamicRoutes = modules.map((m) => {
    const appURL = m.link;
    const remoteEntryURL = combineUrl(
      window.location.origin,
      appURL,
      "remoteEntry.js"
    );

    const system = {
      url: remoteEntryURL,
      scope: m.appName,
      module: "./app",
    };

    window.AppServer.studio.modules[m.appName] = {
      appURL,
      remoteEntryURL,
    };

    return (
      <PrivateRoute
        key={m.id}
        path={appURL}
        component={System}
        system={system}
      />
    );
  });

  //console.log("Shell ", history);

  return (
    <Layout>
      <Router history={history}>
        <>
          {isEditor ? <></> : <NavMenu />}
          <ScrollToTop />
          <Main isDesktop={isDesktop}>
            <Switch>
              <PrivateRoute exact path={HOME_URLS} component={HomeRoute} />
              <PublicRoute exact path={WIZARD_URL} component={WizardRoute} />
              <PrivateRoute path={ABOUT_URL} component={AboutRoute} />
              <PublicRoute exact path={LOGIN_URLS} component={LoginRoute} />
              <Route path={CONFIRM_URL} component={ConfirmRoute} />
              <PrivateRoute
                path={COMING_SOON_URLS}
                component={ComingSoonRoute}
              />
              <PrivateRoute path={PAYMENTS_URL} component={PaymentsRoute} />
              <PrivateRoute
                restricted
                path={SETTINGS_URL}
                component={SettingsRoute}
              />
              <PrivateRoute
                exact
                allowForMe
                path={PROFILE_MY_URL}
                component={MyProfileRoute}
              />
              {dynamicRoutes}
              <PrivateRoute path={ERROR_401_URL} component={Error401Route} />
              <PrivateRoute component={Error404Route} />
            </Switch>
          </Main>
        </>
      </Router>
    </Layout>
  );
};

const ShellWrapper = inject(({ auth }) => {
  const { init, isLoaded } = auth;

  return {
    loadBaseInfo: () => {
      init();
      auth.settingsStore.setModuleInfo(config.homepage, "home");
      auth.setProductVersion(config.version);

      if (auth.settingsStore.isDesktopClient) {
        document.body.classList.add("desktop");
      }
    },
    isLoaded,
    modules: auth.moduleStore.modules,
    isDesktop: auth.settingsStore.isDesktopClient,
  };
})(observer(Shell));

export default () => (
  <ThemeProvider theme={Base}>
    <MobxProvider {...store}>
      <I18nextProvider i18n={i18n}>
        <ShellWrapper />
      </I18nextProvider>
    </MobxProvider>
  </ThemeProvider>
);
