"use client";
import React from "react";
import { NextPage } from "next";
import {
   RegularSignUpModel,
   sleep,
   useGithubSignUpMutation,
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
   Checkbox,
} from "@nextui-org/react";
import * as yup from "yup";
import { Formik } from "formik";
import PasswordInput from "../signin/PasswordInput";
import GoogleSignInButton from "../signin/GoogleSignInButton";
import FacebookSignInButton from "../signin/FacebookSignInButton";
import GithubSignInButton from "../signin/GithubSignInButton";
import { useGoogleSignIn } from "@web/hooks";

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
   acceptTermsAndConditions: yup.boolean().isTrue(),
});

const SignUpPage: NextPage = () => {
   const router = useRouter();
   const { data, mutateAsync: signUp, error } = useRegularSignUpMutation();
   const {
      error: googleSignUpError,
      data: googleLoginData,
      isLoading,
      login,
   } = useGoogleSignIn((r) => router.push(`/`));

   const {
      data: githubData,
      error: githubError,
      isLoading: githubLoading,
      mutateAsync: githubSignUp,
   } = useGithubSignUpMutation();

   async function handleFormSubmit(data: RegularSignUpModel) {
      try {
         await signUp(data);
         await sleep(2000);
         router.push(`/`, { forceOptimisticNavigation: false });
      } catch (e) {
         console.error(e);
      }
   }

   return (
      <div className={`mx-auto mt-24 w-1/3 max-w-[600px]`}>
         <Card shadow={"lg"} className={`p-6`} radius={"md"}>
            <CardHeader className={`text-2xl`}>Create an account</CardHeader>
            <CardBody className={`mt-0`}>
               <Formik<RegularSignUpModel>
                  validationSchema={signUpSchema}
                  initialValues={{
                     email: "",
                     password: "",
                     username: "",
                     acceptTermsAndConditions: false,
                  }}
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
                  }) => {
                     return (
                        <form
                           autoComplete={"off"}
                           noValidate
                           className={`flex  flex-col gap-3`}
                           onSubmit={handleSubmit}
                        >
                           <Input
                              autoFocus
                              isClearable
                              onClear={() => setFieldValue("username", "")}
                              value={values.username}
                              onChange={handleChange}
                              label={"Username"}
                              classNames={{
                                 input: "text-small pb-1",
                                 label: "py-1",
                              }}
                              autoComplete={"off"}
                              aria-autocomplete={`none`}
                              errorMessage={errors?.username}
                              validationState={
                                 touched.username && errors.username
                                    ? "invalid"
                                    : "valid"
                              }
                              placeholder={"How are people going to call you?"}
                              size={"md"}
                              className={`text-md rounded-lg py-1`}
                              type={"text"}
                              name={"username"}
                              id={"username"}
                           />
                           <Input
                              autoFocus
                              isClearable
                              aria-autocomplete={`none`}
                              onClear={() => setFieldValue("username", "")}
                              value={values.email}
                              classNames={{
                                 input: "text-small pb-1",
                                 label: "py-1",
                              }}
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
                              size={"md"}
                              className={`text-md rounded-lg py-1`}
                              type={"email"}
                              name={"email"}
                              id={"email"}
                           />
                           <PasswordInput
                              aria-autocomplete={`none`}
                              size={"md"}
                              value={values.password}
                              classNames={{
                                 input: "text-small pb-1",
                                 label: "py-1",
                              }}
                              onChange={handleChange}
                              errorMessage={errors.password}
                              validationState={
                                 touched.password && errors.password
                                    ? "invalid"
                                    : "valid"
                              }
                              label={"Password"}
                              placeholder={"Choose a strong one"}
                              className={`text-md rounded-lg py-1`}
                              name={"password"}
                              id={"password"}
                           />
                           <Checkbox
                              classNames={{
                                 label: `text-xs leading-4`,
                                 base: `items-start`,
                              }}
                              name={"acceptTermsAndConditions"}
                              required
                              isSelected={values.acceptTermsAndConditions}
                              onChange={handleChange}
                              radius={"sm"}
                              color={"primary"}
                              size={"sm"}
                           >
                              By clicking &quot;Accept,&quot; you acknowledge
                              that you have read, understood, and agree to abide
                              by our{" "}
                              <Link
                                 className={`text-xs`}
                                 size={"sm"}
                                 underline={"none"}
                                 color={"primary"}
                              >
                                 Terms and Conditions.
                              </Link>
                           </Checkbox>
                           <div
                              className={`flex w-full flex-col justify-between`}
                           >
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
                                 className={`mt-8 w-full self-center rounded-md py-5 text-[1.0rem] text-white shadow-sm hover:opacity-80`}
                                 type={`submit`}
                              >
                                 {isSubmitting ? "Loading" : "Sign Up"}
                              </Button>
                              <div className={`mt-1 self-center`}>
                                 <span
                                    className={`text-default-500 text-small`}
                                 >
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
                              {!googleSignUpError && (
                                 <span
                                    className={`text-small text-danger-500 my-2 w-full text-center`}
                                 >
                                    {(googleSignUpError as Error)?.message}{" "}
                                    Please try again.
                                 </span>
                              )}
                              <div
                                 className={`mt-4 flex w-full items-center space-x-4`}
                              >
                                 <Divider
                                    orientation={"horizontal"}
                                    className={"my-6 h-[1px] flex-1"}
                                 />
                                 <span
                                    className={`text-large text-default-600 font-semibold`}
                                 >
                                    OR
                                 </span>
                                 <Divider
                                    orientation={"horizontal"}
                                    className={"my-6 h-[1px] flex-1"}
                                 />
                              </div>
                              <GoogleSignInButton
                                 isLoading={isLoading}
                                 className={`text-small mt-4 w-4/5 self-center py-2`}
                                 onClick={(_) => login()}
                              />
                              <FacebookSignInButton
                                 className={`text-small mt-4 w-4/5 self-center py-2`}
                              />
                              <GithubSignInButton
                                 onError={console.error}
                                 className={`mb-2 w-4/5 self-center transition-opacity duration-200`}
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
                     );
                  }}
               </Formik>
            </CardBody>
         </Card>
      </div>
   );
};

export default SignUpPage;
