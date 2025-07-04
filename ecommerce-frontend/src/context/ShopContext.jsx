import { createContext } from "react";
import store from "../reduxStore/mainStore";
import React from "react";
import { Provider } from "react-redux";
export const ShopContext = createContext();
const ShopContextProvider =(props) => {
    return (
        <Provider store={store}>
        {props.children}
        </Provider>
    )
}

export default ShopContextProvider;