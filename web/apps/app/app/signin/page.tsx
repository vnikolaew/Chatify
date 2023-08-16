"use client";
import React from "react";
import { NextPage } from "next";
import { RegularSignInModel, sleep, useRegularSignInMutation } from "@web/api";
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
} from "@nextui-org/react";
import * as yup from "yup";
import { Formik } from "formik";
import PasswordInput from "./PasswordInput";

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
      .required("Password is required.")
      .matches(PASSWORD_REGEX, { message: "Invalid password." }),
});

const SignInPage: NextPage = () => {
   const router = useRouter();
   const { data, mutateAsync: signIn, error } = useRegularSignInMutation();

   async function handleFormSubmit(data: RegularSignInModel) {
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
            <CardHeader className={`text-2xl`}>
               Sign in with your account
            </CardHeader>
            <CardBody className={`mt-4`}>
               <Formik<RegularSignInModel>
                  validationSchema={signInSchema}
                  initialValues={{ email: "", password: "" }}
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
                        <div className={`w-full flex flex-col justify-between`}>
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
                              className={`text-white mt-12 py-5 w-1/2 text-[1.0rem] hover:opacity-80 self-end shadow-sm rounded-md`}
                              type={`submit`}
                           >
                              {isSubmitting ? "Loading" : "Sign In"}
                           </Button>
                        </div>
                     </form>
                  )}
               </Formik>
            </CardBody>
         </Card>
      </div>
   );
};

export default SignInPage;
