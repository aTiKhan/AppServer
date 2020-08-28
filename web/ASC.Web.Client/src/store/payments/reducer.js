// import {} from "./actions";

const initialState = {
  salesEmail: "sales@onlyoffice.com",
  helpUrl: "https://helpdesk.onlyoffice.com",
  buyUrl: "http://www.onlyoffice.com/post.ashx?type=buyenterprise",
  standaloneMode: true,
  dateExpires: "YYYY-MM-DD[T]HH:mm:ss.SSS[Z]",
  createPortals: "1/2",
};

const paymentsReducer = (state = initialState, action) => {
  switch (action.type) {
    // case
    default:
      return state;
  }
};

export default paymentsReducer;
