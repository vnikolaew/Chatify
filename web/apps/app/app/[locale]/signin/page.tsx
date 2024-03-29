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
import { useRouter, useSearchParams } from "next/navigation";
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
import { Form, Formik } from "formik";
import PasswordInput from "./PasswordInput";
import { FacebookIcon, GithubIcon, GoogleIcon } from "@web/components";
import { useGoogleSignIn } from "@web/hooks";

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
   const returnUrl = useSearchParams().get("returnUrl");

   const {
      error: googleError,
      data: googleData,
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
         await signIn({ ...data, returnUrl });
         await sleep(2000);
         router.push(`/`, { forceOptimisticNavigation: false });
      } catch (e) {
         console.error(e);
      }
   }

   return (
      <div className={`mx-auto mt-24 w-1/3 max-w-[500px]`}>
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
                           className={`text-md rounded-lg py-1`}
                           type={"email"}
                           name={"email"}
                           id={"email"}
                        />
                        <PasswordInput
                           size={"md"}
                           value={values.password}
                           onChange={handleChange}
                           errorMessage={errors.password}
                           isInvalid={Boolean(
                              touched.password && errors.password
                           )}
                           label={"Password"}
                           placeholder={"Choose a strong one"}
                           className={`text-md rounded-lg py-1`}
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
                           className={`mt-2 flex w-full flex-col justify-between`}
                        >
                           <div>
                              <span className={`text-default-500 text-small`}>
                                 Don&apos;t have an account yet?
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
                              className={`mt-12 w-full self-center rounded-md py-5 text-[1.0rem] text-white shadow-sm hover:opacity-80`}
                              type={`submit`}
                           >
                              {isSubmitting ? "Loading" : "Sign In"}
                           </Button>
                           <div
                              className={`mt-2 flex w-full items-center space-x-4`}
                           >
                              <Divider
                                 orientation={"horizontal"}
                                 className={"my-4 h-[1px] flex-1"}
                              />
                              <span
                                 className={`text-medium text-default-600 font-semibold`}
                              >
                                 OR
                              </span>
                              <Divider
                                 orientation={"horizontal"}
                                 className={"my-4 h-[1px] flex-1"}
                              />
                           </div>
                           <div
                              className={`mt-4 flex w-full items-center justify-center gap-4`}
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
                                    <div className={`rounded-xl bg-black p-2`}>
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
