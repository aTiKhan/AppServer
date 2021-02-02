import React, { Suspense, lazy /*useEffect*/ } from "react";
import { Router, Route, Switch } from "react-router-dom";
import { connect } from "react-redux";
import {
  store as CommonStore,
  history,
  PrivateRoute,
  PublicRoute,
  Login,
  Error404,
  Offline,
  ComingSoon,
  NavMenu,
  Main,
  utils,
  toastr,
} from "asc-web-common";
import Home from "./components/pages/Home";
import { inject } from "mobx-react";

const About = lazy(() => import("./components/pages/About"));
const Confirm = lazy(() => import("./components/pages/Confirm"));
const Settings = lazy(() => import("./components/pages/Settings"));
const Wizard = lazy(() => import("./components/pages/Wizard"));
const Payments = lazy(() => import("./components/pages/Payments"));
const ThirdPartyResponse = lazy(() => import("./components/pages/ThirdParty"));
const {
  setIsLoaded,
  //getUser,
  //getPortalSettings,
  //getModules,
  getIsAuthenticated,
} = CommonStore.auth.actions;
//const { userStore, settingsStore } = CommonStore;

class App extends React.Component {
  constructor(props) {
    super(props);

    const pathname = window.location.pathname.toLowerCase();
    this.isThirdPartyResponse = pathname.indexOf("thirdparty") !== -1;
  }
  async componentDidMount() {
    const {
      //getPortalSettings,
      //getUser,
      //getModules,
      setIsLoaded,
      getIsAuthenticated,
      loadBaseInfo,
    } = this.props;

    try {
      const isAuthenticated = await getIsAuthenticated();

      if (isAuthenticated) utils.updateTempContent(isAuthenticated);

      if (this.isThirdPartyResponse) {
        setIsLoaded();
        return;
      }

      await loadBaseInfo();

      utils.updateTempContent();
      setIsLoaded();

      // const requests = [];
      // if (!isAuthenticated) {
      //   requests.push(getPortalSettings());
      // } else if (
      //   !window.location.pathname.includes("confirm/EmailActivation")
      // ) {
      //   requests.push(getUser());
      //   requests.push(getPortalSettings());
      //   //requests.push(getModules());
      // }

      // Promise.all(requests)
      //   .catch((e) => {
      //     toastr.error(e);
      //   })
      //   .finally(() => {
      //     utils.updateTempContent();
      //     setIsLoaded();
      //   });
    } catch (err) {
      toastr.error(err);
    }
  }

  render() {
    return navigator.onLine ? (
      <Router history={history}>
        {!this.isThirdPartyResponse && <NavMenu />}
        <Main>
          <Suspense fallback={null}>
            <Switch>
              <Route exact path="/wizard" component={Wizard} />
              <PublicRoute
                exact
                path={[
                  "/login",
                  "/login/error=:error",
                  "/login/confirmed-email=:confirmedEmail",
                ]}
                component={Login}
              />
              <Route path="/confirm" component={Confirm} />
              <PrivateRoute
                path={`/thirdparty/:provider`}
                component={ThirdPartyResponse}
              />
              <PrivateRoute
                exact
                path={["/", "/error=:error"]}
                component={Home}
              />
              <PrivateRoute exact path="/about" component={About} />
              <PrivateRoute restricted path="/settings" component={Settings} />
              <PrivateRoute
                exact
                path={["/coming-soon"]}
                component={ComingSoon}
              />
              <PrivateRoute path="/payments" component={Payments} />
              <PrivateRoute component={Error404} />
            </Switch>
          </Suspense>
        </Main>
      </Router>
    ) : (
      <Offline />
    );
  }
}

const mapStateToProps = (state) => {
  const { /*modules,*/ isLoaded /* , settings */ } = state.auth;
  //const { organizationName } = settings;
  return {
    //modules,
    isLoaded,
    //organizationName,
  };
};

const mapDispatchToProps = (dispatch) => {
  return {
    getIsAuthenticated: () => getIsAuthenticated(dispatch),
    //getPortalSettings: () => getPortalSettings(dispatch),
    //getUser: () => getUser(dispatch),
    //getModules: () => getModules(dispatch),
    setIsLoaded: () => dispatch(setIsLoaded(true)),
  };
};

// const AppWrapper = inject(({ userStore }, { ...props }) => ({
//   setCurrentUser: userStore.setCurrentUser,
//   user: userStore.user,
//   props,
// }))(
//   observer(({ props, setCurrentUser, user }) => {
//     useEffect(() => {
//       setCurrentUser();
//     }, [setCurrentUser]);

//     return <App {...props} />;
//   })
// );

export default connect(
  mapStateToProps,
  mapDispatchToProps
)(
  inject(({ store }) => {
    return {
      user: store.userStore.user,
      isAuthenticated: store.userStore.isAuthenticated,
      getUser: store.userStore.getCurrentUser,
      getPortalSettings: store.settingsStore.getPortalSettings,
      modules: store.moduleStore.modules,
      loadBaseInfo: store.init,
      //organizationName: settingsStore.settings.organizationName
    };
  })(App)
);

// export default inject(({ userStore }) => ({
//   user: userStore.user,
//   getUser: userStore.setCurrentUser,
// }))(observer(connect(mapStateToProps, mapDispatchToProps)(App)));
