import * as React from "react";
import { render } from "@testing-library/react-native";

import App from "./App";

test("renders correctly", () => {
   const { getByTestId } = render(<App />);
   // @ts-ignore
   expect(getByTestId("heading")).toHaveTextContent("Welcome");
});
