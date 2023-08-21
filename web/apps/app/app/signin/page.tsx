"use client";
import React from "react";
import GithubLogin from "react-github-login";
import { NextPage } from "next";
import {
   RegularSignInModel,
   sleep,
   useGithubSignUpMutation,
   useRegularSignInMutation,
} from "@web/api";
import NextLink from "next/link";
import { useRouter } from "next/navigation";
import {
   Button,
   Card,
   Link,
   CardBody,
   CardHeader,
   Input,
   Spinner,
   Checkbox,
   Divider,
} from "@nextui-org/react";
import * as yup from "yup";
import { Field, Form, Formik } from "formik";
import PasswordInput from "./PasswordInput";
import { GoogleIcon } from "../../components/icons/GoogleIcon";
import { GithubIcon } from "../../components/icons/GithubIcon";
import { FacebookIcon } from "../../components/icons/FacebookIcon";
import { useGoogleSignIn } from "../../hooks/auth/useGoogleSignIn";

const EMAIL_REGEX = /^[a-zA-Z0-9._-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,4}$/;
const PASSWORD_REGEX =
   /^(?=.*\d)(?=.*[A-Z])(?=.*[!@#$%^&*()_+])[A-Za-z\d!@#$%^&*()_+]{3,}$/;

const signInSchema = yup.object({
   email: yup
      .string()
      .max(100, "Email must be less than 100 characters.")
      .required("Email is required.")
      .matches(EMAIL_REGEX, { message: "Invalid email." }),
   password: yup
      .string()
      .min(3, "Password must have more than 3 characters.")
      .max(50, "Password must have less than 50 characters.")
      .required("Password is required."),
   // .matches(PASSWORD_REGEX, { message: "Invalid password." }),
   rememberMe: yup.boolean().required(),
});

const SignInPage: NextPage = () => {
   const router = useRouter();
   const { data, mutateAsync: signIn, error } = useRegularSignInMutation();

   const {
      error: googleError,
      data: googleData,
      isLoading,
      login: googleLogin,
   } = useGoogleSignIn((res) =>
      router.push(`/`, { forceOptimisticNavigation: false })
   );

   const {
      data: githubData,
      error: githubError,
      isLoading: githubLoading,
      mutateAsync: githubSignUp,
   } = useGithubSignUpMutation();

   async function handleFormSubmit(data: RegularSignInModel) {
      try {
         await signIn(data);
         await sleep(2000);
         router.push(`/`, { forceOptimisticNavigation: false });
      } catch (e) {
         console.error(e);
      }
   }

   return (
      <div className={`w-1/3 mx-auto max-w-[500px] mt-24`}>
         <Card shadow={"lg"} className={`p-6`} radius={"md"}>
            <CardHeader className={`text-2xl`}>
               Sign in with your account
            </CardHeader>
            <CardBody className={`mt-4`}>
               <Formik<RegularSignInModel>
                  validationSchema={signInSchema}
                  initialValues={{ email: "", password: "", rememberMe: false }}
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
                     <Form
                        autoComplete={"off"}
                        // noValidate
                        className={`flex flex-col gap-3`}
                        // onSubmit={handleSubmit}
                     >
                        <Input
                           autoFocus
                           isClearable
                           value={values.email}
                           onClear={() => setFieldValue("email", "")}
                           onChange={handleChange}
                           label={"Email"}
                           autoComplete={"off"}
                           errorMessage={errors?.email}
                           validationState={
                              touched.email && errors.email
                                 ? "invalid"
                                 : "valid"
                           }
                           placeholder={"Enter your email"}
                           size={"md"}
                           className={`py-1 text-md rounded-lg`}
                           type={"email"}
                           name={"email"}
                           id={"email"}
                        />
                        <PasswordInput
                           size={"md"}
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
                        <Checkbox
                           name={"rememberMe"}
                           classNames={{
                              label: "text-small text-foreground",
                           }}
                           title={"Remember me"}
                           radius={"md"}
                           color={"primary"}
                        >
                           Remember me
                        </Checkbox>
                        <div
                           className={`w-full mt-2 flex flex-col justify-between`}
                        >
                           <div>
                              <span className={`text-default-500 text-small`}>
                                 Don't have an account yet?
                              </span>
                              <Link
                                 className={`ml-2`}
                                 underline={"hover"}
                                 color={"primary"}
                                 size={"sm"}
                                 as={NextLink}
                                 href={`/signup`}
                              >
                                 Sign up now.
                              </Link>
                           </div>
                           <Button
                              as={"button"}
                              variant={"solid"}
                              isLoading={isSubmitting}
                              spinner={
                                 <Spinner
                                    className={`mr-2`}
                                    color={"white"}
                                    size={"sm"}
                                 />
                              }
                              color={"primary"}
                              size={"sm"}
                              className={`text-white mt-12 py-5 w-full text-[1.0rem] hover:opacity-80 self-center shadow-sm rounded-md`}
                              type={`submit`}
                           >
                              {isSubmitting ? "Loading" : "Sign In"}
                           </Button>
                           <div
                              className={`w-full mt-2 flex items-center space-x-4`}
                           >
                              <Divider
                                 orientation={"horizontal"}
                                 className={"h-[1px] flex-1 my-4"}
                              />
                              <span
                                 className={`text-medium font-semibold text-default-600`}
                              >
                                 OR
                              </span>
                              <Divider
                                 orientation={"horizontal"}
                                 className={"h-[1px] flex-1 my-4"}
                              />
                           </div>
                           <div
                              className={`w-full mt-4 gap-4 justify-center flex items-center`}
                           >
                              <Button
                                 color={"default"}
                                 className={`bg-white`}
                                 onPress={(_) => googleLogin()}
                                 isIconOnly
                                 startContent={<GoogleIcon size={24} />}
                              />
                              <GithubLogin
                                 redirectUri={`http://localhost:4200`}
                                 buttonText={
                                    <div className={`bg-black p-2 rounded-xl`}>
                                       <GithubIcon size={24} />
                                    </div>
                                 }
                                 id={"github"}
                                 clientId={
                                    process.env.NEXT_PUBLIC_GITHUB_CLIENT_ID
                                 }
                                 onError={console.error}
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
                              <Button
                                 color={"primary"}
                                 isIconOnly
                                 startContent={
                                    <FacebookIcon fill={"white"} size={24} />
                                 }
                              />
                           </div>
                        </div>
                     </Form>
                  )}
               </Formik>
            </CardBody>
         </Card>
      </div>
   );
};

export default SignInPage;
