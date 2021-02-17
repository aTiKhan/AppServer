import React, { Suspense, useEffect } from "react";
import { Router, Switch, Redirect } from "react-router-dom";
import Home from "./components/pages/Home";
import Profile from "./components/pages/Profile";
import ProfileAction from "./components/pages/ProfileAction";
import GroupAction from "./components/pages/GroupAction";
import Reassign from "./components/pages/Reassign";
import {
  history,
  PrivateRoute,
  PublicRoute,
  Login,
  Error404,
  Error520,
  Offline,
  utils,
  NavMenu,
  Main,
  toastr,
} from "asc-web-common";
import config from "../package.json";
import { inject, observer } from "mobx-react";

// const Profile = lazy(() => import("./components/pages/Profile"));
// const ProfileAction = lazy(() => import("./components/pages/ProfileAction"));
// const GroupAction = lazy(() => import("./components/pages/GroupAction"));

const App = (props) => {
  const { homepage, isLoaded, loadBaseInfo } = props;

  useEffect(() => {
    try {
      loadBaseInfo();
    } catch (err) {
      toastr.error(err);
    }
  }, [loadBaseInfo]);

  useEffect(() => {
    if (isLoaded) utils.updateTempContent();
  }, [isLoaded]);

  return navigator.onLine ? (
    <Router history={history}>
      <NavMenu />
      <Main>
        <Suspense fallback={null}>
          <Switch>
            <Redirect exact from="/" to={`${homepage}`} />
            <PrivateRoute
              exact
              path={`${homepage}/view/:userId`}
              component={Profile}
            />
            <PrivateRoute
              path={`${homepage}/edit/:userId`}
              restricted
              allowForMe
              component={ProfileAction}
            />
            <PrivateRoute
              path={`${homepage}/create/:type`}
              restricted
              component={ProfileAction}
            />
            <PrivateRoute
              path={[
                `${homepage}/group/edit/:groupId`,
                `${homepage}/group/create`,
              ]}
              restricted
              component={GroupAction}
            />
            <PrivateRoute
              path={`${homepage}/reassign/:userId`}
              restricted
              component={Reassign}
            />
            <PrivateRoute exact path={homepage} component={Home} />
            <PrivateRoute path={`${homepage}/filter`} component={Home} />
            <PublicRoute
              exact
              path={[
                "/login",
                "/login/error=:error",
                "/login/confirmed-email=:confirmedEmail",
              ]}
              component={Login}
            />
            <PrivateRoute exact path={`/error=:error`} component={Error520} />
            <PrivateRoute component={Error404} />
          </Switch>
        </Suspense>
      </Main>
    </Router>
  ) : (
    <Offline />
  );
};

export default inject(({ auth, peopleStore }) => ({
  homepage: auth.settingsStore.homepage || config.homepage,
  loadBaseInfo: () => {
    auth.init();
    peopleStore.init();
  },
  isLoaded: auth.isLoaded,
}))(observer(App));
