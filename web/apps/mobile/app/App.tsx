import React from "react";
import { StyleSheet } from "react-native";
import {
   QueryCache,
   QueryClient,
   QueryClientProvider,
} from "@tanstack/react-query";
import SignInPage from "../components/SignInPage";
import { NavigationContainer } from "@react-navigation/native";
import { createNativeStackNavigator } from "@react-navigation/native-stack";
import { GestureHandlerRootView } from "react-native-gesture-handler";
import "react-native-gesture-handler";

const Stack = createNativeStackNavigator();

export const App = () => {
   return (
      <QueryClientProvider
         client={
            new QueryClient({
               queryCache: new QueryCache(),
               defaultOptions: {
                  queries: {
                     staleTime: 10 * 1000 * 60,
                  },
               },
            })
         }
      >
         <GestureHandlerRootView style={{ flex: 1 }}>
            <NavigationContainer>
               <Stack.Navigator>
                  <Stack.Screen component={SignInPage} name={`SignIn`} />
               </Stack.Navigator>
            </NavigationContainer>
         </GestureHandlerRootView>
      </QueryClientProvider>
   );
};

const styles = StyleSheet.create({
   scrollView: {
      backgroundColor: "#ffffff",
   },
   codeBlock: {
      backgroundColor: "rgba(55, 65, 81, 1)",
      marginVertical: 12,
      padding: 12,
      borderRadius: 4,
   },
   monospace: {
      color: "#ffffff",
      marginVertical: 4,
   },
   comment: {
      color: "#cccccc",
   },
   marginBottomSm: {
      marginBottom: 6,
   },
   marginBottomMd: {
      marginBottom: 18,
   },
   marginBottomLg: {
      marginBottom: 24,
   },
   textLight: {
      fontWeight: "300",
   },
   textBold: {
      fontWeight: "500",
   },
   textCenter: {
      textAlign: "center",
   },
   text2XS: {
      fontSize: 12,
   },
   textXS: {
      fontSize: 14,
   },
   textSm: {
      fontSize: 16,
   },
   textMd: {
      fontSize: 18,
   },
   textLg: {
      fontSize: 24,
   },
   textXL: {
      fontSize: 48,
   },
   textContainer: {
      marginVertical: 12,
   },
   textSubtle: {
      color: "#6b7280",
   },
   section: {
      marginVertical: 24,
      marginHorizontal: 12,
   },
   shadowBox: {
      backgroundColor: "white",
      borderRadius: 24,
      shadowColor: "black",
      shadowOpacity: 0.15,
      shadowOffset: {
         width: 1,
         height: 4,
      },
      shadowRadius: 12,
      padding: 24,
      marginBottom: 24,
   },
   listItem: {
      display: "flex",
      flexDirection: "row",
      alignItems: "center",
   },
   listItemTextContainer: {
      marginLeft: 12,
      flex: 1,
   },
   appTitleText: {
      paddingTop: 12,
      fontWeight: "500",
   },
   hero: {
      borderRadius: 12,
      backgroundColor: "#143055",
      padding: 36,
      marginBottom: 24,
   },
   heroTitle: {
      flex: 1,
      flexDirection: "row",
   },
   heroTitleText: {
      color: "#ffffff",
      marginLeft: 12,
   },
   heroText: {
      color: "#ffffff",
      marginVertical: 12,
   },
   whatsNextButton: {
      backgroundColor: "#ffffff",
      paddingVertical: 16,
      borderRadius: 8,
      width: "50%",
      marginTop: 24,
   },
   learning: {
      marginVertical: 12,
   },
   love: {
      marginTop: 12,
      justifyContent: "center",
   },
});

export default App;
