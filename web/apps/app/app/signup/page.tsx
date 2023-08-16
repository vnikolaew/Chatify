"use client";
import React from "react";
import { NextPage } from "next";
import {
   RegularSignUpModel,
   sleep,
   useGithubSignUpMutation,
   useGoogleSignUpMutation,
   useRegularSignUpMutation,
} from "@web/api";
import NextLink from "next/link";
import { useRouter } from "next/navigation";
import {
   Button,
   Link,
   Card,
   CardBody,
   CardHeader,
   Input,
   Spinner,
   Divider,
} from "@nextui-org/react";
import * as yup from "yup";
import { Formik } from "formik";
import PasswordInput from "../signin/PasswordInput";
import GoogleSignInButton from "../signin/GoogleSignInButton";
import FacebookSignInButton from "../signin/FacebookSignInButton";
import { useGoogleLogin } from "@react-oauth/google";
import GithubSignInButton from "../signin/GithubSignInButton";
import axios from "axios";

const EMAIL_REGEX = /^[a-zA-Z0-9._-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,4}$/;
const PASSWORD_REGEX =
   /^(?=.*\d)(?=.*[A-Z])(?=.*[!@#$%^&*()_+])[A-Za-z\d!@#$%^&*()_+]{3,}$/;

const signUpSchema = yup.object({
   email: yup
      .string()
      .max(100, "Email must be less than 100 characters.")
      .required("Email is required.")
      .matches(EMAIL_REGEX, { message: "Invalid email." }),
   username: yup
      .string()
      .min(3, "Username must have more than 3 characters.")
      .max(50, "Username must have less than 50 characters.")
      .required("Username is required."),
   password: yup
      .string()
      .min(3, "Password must have more than 3 characters.")
      .max(50, "Password must have less than 50 characters.")
      .required("Password is required.")
      .matches(PASSWORD_REGEX, { message: "Invalid password." }),
});

const SignUpPage: NextPage = () => {
   const router = useRouter();
   const { data, mutateAsync: signUp, error } = useRegularSignUpMutation();
   const {
      error: googleSignUpError,
      data: googleLoginData,
      isLoading,
      mutateAsync: googleSignUp,
   } = useGoogleSignUpMutation();
   const {
      data: githubData,
      error: githubError,
      isLoading: githubLoading,
      mutateAsync: githubSignUp,
   } = useGithubSignUpMutation();

   const login = useGoogleLogin({
      onSuccess: ({ access_token, scope }) => {
         googleSignUp(
            { accessToken: access_token },
            {
               onError: (err) =>
                  console.error("A sign up error occurred: ", err),
            }
         )
            .then((r) => {
               console.log("Google login data: ", googleLoginData);
               router.push(`/`);
            })
            .catch((err) => {
               console.log("Google login error: ", googleSignUpError);
            });
      },
      scope: "https://www.googleapis.com/auth/userinfo.profile",
      flow: "implicit",
      onError: console.error,
   });

   async function handleFormSubmit(data: RegularSignUpModel) {
      try {
         // await signIn(signInModel);
         await sleep(2000);
         // router.push(`/`, { forceOptimisticNavigation: false });
      } catch (e) {
         console.error(e);
      }
   }

   return (
      <div className={`w-1/3 mx-auto mt-24`}>
         <Card shadow={"lg"} className={`p-6`} radius={"md"}>
            <CardHeader className={`text-2xl`}>Create an account</CardHeader>
            <CardBody className={`mt-0`}>
               <Formik<RegularSignUpModel>
                  validationSchema={signUpSchema}
                  initialValues={{ email: "", password: "", username: "" }}
                  validateOnMount={false}
                  validateOnChange={true}
                  onSubmit={handleFormSubmit}
               >
                  {({
                     values,
                     errors,
                     setFieldValue,
                     touched,
                     handleChange,
                     handleSubmit,
                     isSubmitting,
                  }) => (
                     <form
                        autoComplete={"off"}
                        noValidate
                        className={`flex flex-col gap-3`}
                        onSubmit={handleSubmit}
                     >
                        <Input
                           autoFocus
                           isClearable
                           onClear={() => setFieldValue("username", "")}
                           value={values.username}
                           onChange={handleChange}
                           label={"Username"}
                           autoComplete={"off"}
                           errorMessage={errors?.username}
                           validationState={
                              touched.username && errors.username
                                 ? "invalid"
                                 : "valid"
                           }
                           placeholder={"How are people going to call you?"}
                           size={"sm"}
                           className={`py-1 text-md rounded-lg`}
                           type={"text"}
                           name={"username"}
                           id={"username"}
                        />
                        <Input
                           autoFocus
                           isClearable
                           onClear={() => setFieldValue("username", "")}
                           value={values.email}
                           onChange={handleChange}
                           description={
                              "We will never share your email with anybody else."
                           }
                           label={"Email"}
                           autoComplete={"off"}
                           errorMessage={errors?.email}
                           validationState={
                              touched.email && errors.email
                                 ? "invalid"
                                 : "valid"
                           }
                           placeholder={"Type in your email"}
                           size={"sm"}
                           className={`py-1 text-md rounded-lg`}
                           type={"email"}
                           name={"email"}
                           id={"email"}
                        />
                        <PasswordInput
                           size={"sm"}
                           value={values.password}
                           onChange={handleChange}
                           errorMessage={errors.password}
                           validationState={
                              touched.password && errors.password
                                 ? "invalid"
                                 : "valid"
                           }
                           label={"Password"}
                           placeholder={"Choose a strong one"}
                           className={`py-1 text-md rounded-lg`}
                           name={"password"}
                           id={"password"}
                        />
                        <div className={`w-full flex flex-col justify-between`}>
                           <div>
                              <span className={`text-default-500 text-small`}>
                                 Already have an account?
                              </span>
                              <Link
                                 underline={"hover"}
                                 as={NextLink}
                                 className={`text-small text-primary ml-2`}
                                 href={`/signin`}
                              >
                                 Sign in.
                              </Link>
                           </div>
                           <Button
                              variant={"solid"}
                              isLoading={isSubmitting}
                              spinner={
                                 <Spinner
                                    className={`mr-2`}
                                    color={"default"}
                                    size={"sm"}
                                 />
                              }
                              color={"primary"}
                              size={"sm"}
                              className={`text-white w-full mt-12 py-5 text-[1.0rem] hover:opacity-80 self-center shadow-sm rounded-md`}
                              type={`submit`}
                           >
                              {isSubmitting ? "Loading" : "Sign Up"}
                           </Button>
                           {!googleSignUpError && (
                              <span
                                 className={`text-small my-2 w-full text-center text-danger-500`}
                              >
                                 {(googleSignUpError as Error)?.message} Please
                                 try again.
                              </span>
                           )}
                           <div
                              className={`w-full mt-4 flex items-center space-x-4`}
                           >
                              <Divider
                                 orientation={"horizontal"}
                                 className={"h-[1px] flex-1 my-6"}
                              />
                              <span
                                 className={`text-large font-semibold text-default-600`}
                              >
                                 OR
                              </span>
                              <Divider
                                 orientation={"horizontal"}
                                 className={"h-[1px] flex-1 my-6"}
                              />
                           </div>
                           <GoogleSignInButton
                              isLoading={isLoading}
                              className={`w-4/5 self-center mt-4 py-2 text-small`}
                              onClick={(_) => login()}
                           />
                           <FacebookSignInButton
                              className={`w-4/5 mt-4 self-center py-2 text-small`}
                           />
                           <GithubSignInButton
                              onError={console.error}
                              className={`mb-2 transition-opacity duration-200 self-center w-4/5`}
                              onSuccess={async ({ code }) => {
                                 githubSignUp({ code })
                                    .then((res) => {
                                       router.push(`/`, {
                                          forceOptimisticNavigation: false,
                                       });
                                    })
                                    .catch((err) => {
                                       console.error(err);
                                       console.error(githubError);
                                    });
                              }}
                           />
                        </div>
                     </form>
                  )}
               </Formik>
            </CardBody>
            {/*<CardFooter></CardFooter>*/}
         </Card>
      </div>
   );
};

export default SignUpPage;
