import React, { useEffect, useState } from "react";
import {
   SafeAreaView,
   Text,
   StyleSheet,
   View,
   TextInput,
   TouchableOpacity,
   Image,
   ActivityIndicator,
   TouchableHighlight,
} from "react-native";
import {
   RegularSignInModel,
   useGetMyClaimsQuery,
   useGetUserDetailsQuery,
   useRegularSignInMutation,
   useSignOutMutation,
} from "@web/api";
import AsyncStorage from "@react-native-async-storage/async-storage";
import Checkbox from "expo-checkbox";
import { Colors } from "../constants/colors";
import GoogleIcon from "./icons/GoogleIcon";
import GithubIcon from "./icons/GithubIcon";
import FacebookIcon from "./icons/FacebookIcon";
import * as WebBrowser from "expo-web-browser";

export interface TestProps {}

WebBrowser.maybeCompleteAuthSession();

const SignInPage = ({}: TestProps) => {
   const [signInValues, setSignInValues] = useState<RegularSignInModel>({
      email: ``,
      password: ``,
      rememberMe: false,
   });
   const {
      mutateAsync: signInAsync,
      error,
      data,
      isPending: signInLoading,
   } = useRegularSignInMutation();
   const {
      data: me,
      isLoading,
      error: _,
      refetch,
   } = useGetMyClaimsQuery({ enabled: data !== undefined && data !== null });
   const [isSignUpHighlighted, setIsSignUpHighlighted] = useState(false);

   const {
      data: userDetails,
      error: userError,
      isLoading: userLoading,
   } = useGetUserDetailsQuery(me?.claims?.["nameidentifier"], {
      enabled: !!me,
   });
   const { mutateAsync: signOutAsync, isPending: signOutLoading } =
      useSignOutMutation();

   useEffect(() => {
      AsyncStorage.getItem(`auth_cookie`, (err, cookie) => {
         if (err) console.error(err);
      }).catch(console.error);
   }, [me]);

   async function handleSignIn() {
      await signInAsync(signInValues, {
         onSuccess: async (res) => {
            await refetch()
               .then((res) => {})
               .catch((err) => console.error(err));
         },
         onError: console.error,
      });
   }

   async function handleSignOut() {
      await signOutAsync({});
   }

   return (
      <SafeAreaView style={[styles.page]}>
         <View
            style={{
               flex: 1,
               flexDirection: `column`,
               alignItems: `center`,
               justifyContent: `center`,
               gap: 24,
               marginHorizontal: 24,
               marginBottom: 120,
            }}
         >
            <Text
               style={{
                  color: `#fff`,
                  fontSize: 20,
                  alignSelf: `flex-start`,
                  marginLeft: 32,
               }}
            >
               Sign in with your account
            </Text>
            <View style={styles.inputContainer}>
               <Text style={styles.label}>Email</Text>
               <TextInput
                  textContentType={`emailAddress`}
                  autoComplete={`email`}
                  keyboardType={`email-address`}
                  value={signInValues.email}
                  onChangeText={(value) =>
                     setSignInValues((x) => ({ ...x, email: value }))
                  }
                  style={styles.input}
                  placeholder={`Enter your e-mail`}
                  nativeID={`e-mail`}
               />
            </View>
            <View style={styles.inputContainer}>
               <Text style={styles.label}>Password</Text>
               <TextInput
                  secureTextEntry
                  textContentType={`newPassword`}
                  autoComplete={`new-password`}
                  keyboardType={`visible-password`}
                  value={signInValues.password}
                  onChangeText={(value) =>
                     setSignInValues((x) => ({ ...x, password: value }))
                  }
                  style={styles.input}
                  placeholder={`Choose a string one`}
                  nativeID={`password`}
               />
            </View>
            <View
               style={{
                  width: `80%`,
                  gap: 6,
                  flexDirection: `row`,
                  alignItems: `center`,
               }}
            >
               <Checkbox
                  style={styles.checkbox}
                  value={signInValues.rememberMe}
                  onValueChange={(value) =>
                     setSignInValues((x) => ({ ...x, rememberMe: value }))
                  }
                  color={Colors.blue}
               />
               <Text style={{ color: Colors.white, fontSize: 12 }}>
                  Remember me
               </Text>
            </View>
            <View
               style={{
                  width: `80%`,
                  gap: 8,
                  flexDirection: `row`,
                  alignItems: `center`,
               }}
            >
               <Text style={{ color: Colors.white, fontSize: 12 }}>
                  Don't have an account yet?
               </Text>
               <TouchableHighlight
                  onPressIn={() => setIsSignUpHighlighted(true)}
                  onPressOut={() => setIsSignUpHighlighted(false)}
                  underlayColor={`transparent`}
               >
                  <Text
                     style={{
                        color: Colors.blue,
                        fontSize: 12,
                        textDecorationLine: isSignUpHighlighted
                           ? `underline`
                           : `none`,
                     }}
                  >
                     Sign up now.
                  </Text>
               </TouchableHighlight>
            </View>
            <View style={{ width: `80%`, gap: 4 }}>
               <TouchableOpacity onPress={handleSignIn} style={styles.button}>
                  {signInLoading ? (
                     <ActivityIndicator
                        animating={signInLoading}
                        style={{ marginVertical: 0 }}
                        color={`#fff`}
                        size={17}
                     />
                  ) : (
                     <Text style={{ color: `#fff`, alignSelf: `center` }}>
                        Sign in
                     </Text>
                  )}
               </TouchableOpacity>
            </View>
            <View>
               <View
                  style={[
                     styles.fullWidth,
                     {
                        flexDirection: `row`,
                        gap: 12,
                        alignItems: `center`,
                        width: `80%`,
                     },
                  ]}
               >
                  <View style={styles.separator} />
                  <Text style={[styles.textWhite, styles.textBold]}>OR</Text>
                  <View style={styles.separator} />
               </View>
            </View>
            <View
               style={{
                  marginTop: 8,
                  width: `80%`,
                  flexDirection: `row`,
                  justifyContent: `center`,
                  alignItems: `center`,
                  gap: 12,
               }}
            >
               <TouchableOpacity
                  activeOpacity={0.9}
                  onPress={async () => {}}
                  style={{
                     backgroundColor: Colors.white,
                     padding: 8,
                     borderRadius: 8,
                  }}
               >
                  <GoogleIcon size={16} />
               </TouchableOpacity>

               <TouchableOpacity
                  activeOpacity={0.9}
                  style={{
                     backgroundColor: Colors.black,
                     padding: 8,
                     borderRadius: 8,
                  }}
               >
                  <GithubIcon size={16} />
               </TouchableOpacity>
               <TouchableOpacity
                  activeOpacity={0.9}
                  style={{
                     backgroundColor: Colors.blue,
                     padding: 8,
                     borderRadius: 8,
                  }}
               >
                  <FacebookIcon fill={Colors.white} size={16} />
               </TouchableOpacity>
            </View>
            <View style={{ width: `80%`, gap: 4 }}>
               {userDetails && (
                  <View
                     style={{
                        flexDirection: `row`,
                        gap: 8,
                        alignItems: `center`,
                     }}
                  >
                     <Image
                        source={{
                           uri: userDetails.user?.profilePicture?.mediaUrl!,
                        }}
                        style={{
                           borderRadius: 50,
                           shadowRadius: 20,
                           shadowOpacity: 10,
                        }}
                        width={40}
                        height={40}
                     />
                     <Text style={{ color: `#fff`, fontSize: 14 }}>
                        {userDetails.user?.username}
                     </Text>
                  </View>
               )}
            </View>
         </View>
      </SafeAreaView>
   );
};

const styles = StyleSheet.create({
   page: {
      flex: 1,
      backgroundColor: Colors.gray,
   },
   fullWidth: {
      width: `100%`,
   },
   textWhite: {
      color: Colors.white,
   },
   textBold: {
      fontWeight: "bold",
   },
   textGray: {
      color: Colors.gray2,
   },
   label: {
      fontSize: 14,
      color: `#aaa`,
   },
   button: {
      borderRadius: 8,
      // marginTop: 12,
      width: `100%`,
      paddingVertical: 8,
      paddingHorizontal: 12,
      alignSelf: `center`,
      justifyContent: `center`,
      textAlign: `center`,
      backgroundColor: Colors.blue,
      color: Colors.white,
   },
   checkbox: {
      width: 16,
      height: 16,
      borderRadius: 6,
   },

   inputContainer: {
      width: `80%`,
      gap: 2,
   },
   input: {
      width: `100%`,
      // marginTop: 4,
      backgroundColor: Colors.white,
      borderRadius: 6,
      paddingHorizontal: 8,
      paddingVertical: 2,
      fontSize: 12,
   },
   separator: {
      borderRadius: 8,
      height: 1,
      backgroundColor: Colors.gray2,
      flex: 1,
   },
});

export default SignInPage;
