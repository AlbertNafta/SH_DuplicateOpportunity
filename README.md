# SH_DuplicateOpportunity
""# Opportunity Clone Requirement

## Requirement

* A ribbon button named **Clone** will appear on the **Opportunity** form.
* Clicking the button will:

  * Trigger a JavaScript function that executes a custom Action.
  * The custom Action will call a plugin, written using early-bound classes, to:

    * Duplicate the Opportunity record.
    * Copy relevant fields like topic, description, estimated revenue, and associated products.
    * Maintain any linked Stakeholders or associated records as appropriate.
* The cloned Opportunity will:

  * Be opened automatically (optional).
  * Have a default name such as `Cloned - [Original Name]`.

---

## Plugin Details

Plugins are triggered and executed automatically based on the CRUD operations. However, you can bind the plugin into an **on-demand plugin** by using **Custom Action**.

Binding the plugin to the Custom Action allows you to attach that Custom Action to a **JS or Power Automate**, which enables it to be executed on-demand.

### Helpful Resources:

* [Guide for creating a plugin + custom action + binding them together](https://community.dynamics.com/forums/thread/details/?threadid=34444b0a-0fdf-40e8-acbc-3f66f93f0d5d)
* [YouTube tutorial for Custom Action and Plugin binding](https://youtu.be/PH62f1KjKNo?si=fEL5kqQuoK599XnW)
* [Execute Custom Action using Xrm.WebApi](https://learn.microsoft.com/en-us/power-apps/developer/model-driven-apps/clientapi/reference/xrm-webapi/online/execute)

---

## Tools & Tutorials:

* **Dataverse REST Builder:** [Repository and Readme](https://github.com/GuidoPreite/DRB?tab=readme-ov-file)
* **Main Tutorial:** [YouTube Playlist](https://youtube.com/playlist?list=PLkrU1pMH54uzGFGfhvgqra2pHR6KJ8qNG&si=uRgA_8oQxs77KRRZ)
* **Command Bar Button Creation:** [YouTube Guide](https://youtu.be/xgOF8gE5el8?si=XU1bqFSwO-h-ZctC)

---

## Understanding Plugins

Plugins that are bound or unbound are based on the `Target` keyword:

* If there is a `Target` keyword → **Bound Plugin**
* If there is no `Target` keyword, and you need to pass a `Logical Name` → **Unbound Plugin**

### Additional Notes:

* Bound action is for bound plugins with a single entity/data.
* Unbound action is for unbound plugins with multiple entities/data.
* Workarounds exist for loops in bound actions to access multiple records.
* Mixed bound and unbound actions are generally not seen.

### Helpful Links:

* [Bound vs Unbound](https://chatgpt.com/share/6801ae48-9bd0-8010-a7f2-6b1b5226784a)
* [Unbound action with unbound plugin using late-bound](https://chatgpt.com/share/6801a49a-da44-8002-b6bb-f028452aa7fc)
* [Cannot mix bound and unbound](https://chatgpt.com/share/6801b3f3-5e58-8010-83cd-63b104fa96c3)

---

## Early-bound and Late-bound Plugins:

* [Early-bound Plugin Documentation](https://learn.microsoft.com/en-us/power-apps/developer/data-platform/write-plug-in?tabs=iplugin#using-early-bound-types-in-plug-in-code)
* [Early-bound Programming](https://learn.microsoft.com/en-us/power-apps/developer/data-platform/org-service/early-bound-programming)
* [Mixing Early and Late-bound](https://learn.microsoft.com/en-us/power-apps/developer/data-platform/org-service/early-bound-programming#mix-early-and-late-bound)

---

## Debugging:

* [How to Debug Plugins](https://youtu.be/0Enh1IktS28?si=P7jSa7g69HPuPYxq)
* Turn on **tracing log** before performing any logging/tracing:

  * [Classic UI Trace Log](https://youtu.be/Mx0UQYj1KIA?si=CDIeB3_vI6pAw5Nn)

---

## Additional References:

* [Understanding the Data Context](https://learn.microsoft.com/en-us/power-apps/developer/data-platform/understand-the-data-context)
* [IPluginExecutionContext Documentation](https://learn.microsoft.com/en-us/dotnet/api/microsoft.xrm.sdk.ipluginexecutioncontext?view=dataverse-sdk-latest)
* [Generate Early-bound Classes](https://youtu.be/WbbI4YcXA5M?si=inP81i1SU7DsFv-A)
  ""
---
---
""# ALL THINGS - EVERYTHING ABOUT CUSTOM ACTION

---

## What is Custom Action?

There are some processes that execute automatically (e.g., plugins) that do not support on-demand execution. **Custom Action** enables those automated processes to execute on-demand via **JavaScript** or **Power Automate**.

---

## Types of Custom Actions

1. **Bound Custom Action**

   * Action that is bound to an entity or a set of rows of an entity.

2. **Unbound Custom Action**

   * Action that is not bound to an entity; it’s environment-wide.

---

## Troubleshooting

If you don’t find your Custom Actions inside **XRMToolBox - Registration Plugin Tool**, try the following:

* Activate and publish the Custom Action.
* Relaunch the XRMToolBox, closing and opening it again.
* If not found in **DRB**, turn it on + publish it, then relaunch the app and everything.

---

## Clone with Relationship

**Clone with a BPF table:** [Link](https://chatgpt.com/share/6804a4af-f034-8010-8611-a01f1fe9718f)

The example above shows how to clone an entity/record with a **BPF**. However, during my implementation of **Opportunity of Sales Hub**, I found that when you clone an **Opportunity** record, it will also automatically clone/create the BPF with data. This does not include the **stage process**; you have to handle that separately.

* My guess is because **Sales Process BPF** is bound to **Opportunity**, so whenever an Opportunity is created, a **Sales Process BPF** is also created.

I also noticed some **Opportunities** have the BPF called **Lead to Opportunity** instead of **Sales Process**. When you clone **Opportunity**, it will clone/create a **Sales Process BPF** instead of this **Lead to Opportunity BPF**, but the data is preserved.

* This is because of the bound logic mentioned before. I believe **Lead** is also bound to **Lead to Opportunity BPF**.
* To create it automatically, you need to create/qualify a **Lead** or you can add a **LeadId** to your **Lead to Opportunity BPF** during creation.
* To clone the **stage process**, fetch the original stage, then copy that to the cloned BPF (you might want to fetch the cloned data as well).
* Some **Opportunities** do not have the **Sales Process**; instead, they have **Lead to Opportunity**. You must add a condition to check if there is a **Sales Process BPF** or not.

---

## Error with Registering Plugin

[Reference](https://chatgpt.com/share/681055dc-f5a0-8010-84f7-39543a8e92e3)
When you create your plugin, it will be with **.NET 4.7.2**, which may cause errors because it isn’t supported.

* **Solution:** Change the project to **.NET 4.7.1** or **.NET 4.7**.

---

## Lesson: Bypassing Plugin Max Capacity of 16MB

A plugin can only have a **max capacity of 16MB**, which is defined by the compiled size of the `.dll` file after building the solution.

### Strategy to bypass:

* Divide the plugins into **multiple projects**.
* Old structure:

  * `PluginSolution > PluginProject > AccountPlugin > ContactPlugin` → Single `.dll`.
* New structure:

  * `PluginSolution > AccountPluginProject + ContactPluginProject` → Multiple `.dll` files, increasing the 16MB limit.

### Reusable Components:

* You can create a file with reusable components and use those across multiple plugins.
* This only works when all the plugins are defined within **one project**.
* If plugins are defined in **multiple projects**, you need a **dependent plugin** to share functions.

---

## Helpful Links

* [Step-by-Step Dataverse Dependent Assemblies](https://rajeevpentyala.com/2022/08/30/step-by-step-dataverse-dependent-assemblies/)
* [Dependent Assemblies Documentation](https://learn.microsoft.com/en-us/power-platform-release-plan/2022wave1/data-platform/dependent-assemblies-plug-ins)
* [Difference between Virtual Intersect Table and Custom Intersect Table](https://g.co/gemini/share/57010dcb7bea)

---

## Learning and Debugging

* [Debug a Plugin](https://youtu.be/0Enh1IktS28?si=HRM-fzzw_9E-ruaP)
* [Generate Early-bound Classes](https://youtu.be/WbbI4YcXA5M?si=XhCC8x5d39eOXqsd)
* [Power Automate to Call External API](https://youtu.be/weAMQKf2C6o?si=rA_NKi4Yl4hveTUg)
* [Plugin to Call External API](https://youtu.be/XSxtLZs2lSM?si=HmBbL5ERB7cm3x5F)

---

## References

* [Dovydas Grigaitis YouTube Channel](https://www.youtube.com/@DovydasGrigaitis)
* [Power Apps Solutions in Minutes YouTube Channel](https://www.youtube.com/@powerappssolutionsinminutes)
* [Web Access Plugin Sample](https://learn.microsoft.com/en-us/power-apps/developer/data-platform/org-service/samples/web-access-plugin)
* [Accessing Web Services](https://learn.microsoft.com/en-us/power-apps/developer/data-platform/access-web-services)
  ""
